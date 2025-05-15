using UnityEngine;
using UnityEditor;
using System.IO;
using UnityEditor.Animations;
using System.Collections.Generic;
using System.Linq;
using Lacobus.Animation;


namespace Lacobus_Editors.Animation
{

    [CustomEditor(typeof(AnimationHandlerComponent))]
    public class AnimationControllerEditor : EditorUtils<AnimationHandlerComponent>
    {
        SerializedProperty _animator;
        SerializedProperty _clipPath;
        SerializedProperty _animatorControllerSource;
        SerializedProperty _clipDataArray;
        SerializedProperty _selection;

        Animator _animatorCache;
        AnimatorController _cachedController;

        private void InpectorUpdate()
        {
            _animator = GetProperty("_animator");
            _clipPath = GetProperty("_animationClipSource");
            _animatorControllerSource = GetProperty("_animatorControllerSource");
            _clipDataArray = GetProperty("_clipDataArray");
            _selection = GetProperty("_selection");

            // Add animator compoenent if missing
            _animatorCache = Root.GetComponent<Animator>();
            if (_animatorCache == null)
            {
                _animatorCache = Root.gameObject.AddComponent<Animator>();
                _animator.objectReferenceValue = _animatorCache;
            }
            else if (_animator.objectReferenceValue == null)
            {
                _animator.objectReferenceValue = _animatorCache;
            }

            Heading("Animation Settings");
            Space(10);

            BeginHorizontal();

            PropertyField(_clipPath, "Animation Clips Filepath ", "This is where all the animation clips are saved");
            if (string.IsNullOrEmpty(_clipPath.stringValue) && Button("Locate folder"))
                _clipPath.stringValue = trimPath(EditorUtility.OpenFolderPanel("Animation Clip Source", "Assets/", ""));

            EndHorizontal();

            if (!Directory.Exists(_clipPath.stringValue))
            {
                _clipPath.stringValue = null;
                return;
            }
            else if (Directory.GetFiles(_clipPath.stringValue, "*.anim").Length == 0)
            {
                Info("Directory is empty, no animation clips found", MessageType.Info);
            }
            else
            {
                Space(5);
                Heading("Available animation clips");
                Space(10);

                _clipDataArray.ClearArray();
                foreach (var path in Directory.EnumerateFiles(_clipPath.stringValue, "*.anim"))
                {
                    var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
                    int i = _clipDataArray.arraySize;

                    _clipDataArray.InsertArrayElementAtIndex(i++);
                    _clipDataArray.GetArrayElementAtIndex(i - 1).FindPropertyRelative("clipName").stringValue = clip.name;
                    _clipDataArray.GetArrayElementAtIndex(i - 1).FindPropertyRelative("isLooping").boolValue = clip.isLooping;

                    EditorGUILayout.LabelField($"Clip name : {clip.name}", $"Is looping : {clip.isLooping}");
                }
            }

            Space(20);
            BeginHorizontal();

            PropertyField(_animatorControllerSource, "Animator Controller Filepath", "This is where the animator controller is saved");
            if (string.IsNullOrEmpty(_animatorControllerSource.stringValue))
            {
                if (Button("Locate folder"))
                {
                    _animatorControllerSource.stringValue = trimPath(EditorUtility.OpenFolderPanel("Animator Controller Folder ", _clipPath.stringValue, ""));
                }
                else if (Button("Create folder"))
                {
                    _animatorControllerSource.stringValue = _clipPath.stringValue + @"/Animator Controller/";
                    Directory.CreateDirectory(_clipPath.stringValue + @"/Animator Controller/");
                }
            }

            EndHorizontal();

            if (!Directory.Exists(_animatorControllerSource.stringValue))
            {
                _animatorControllerSource.stringValue = null;
                return;
            }
            else if (Directory.GetFiles(_animatorControllerSource.stringValue, "*.controller").Length == 0)
            {
                Space(15);
                if (Button("No controller found, create new"))
                {
                    _cachedController = AnimatorController.CreateAnimatorControllerAtPath($"{_animatorControllerSource.stringValue}/{Root.gameObject.name}.controller");
                    _animatorCache.runtimeAnimatorController = _cachedController;
                }
                return;
            }
            else
            {
                if (_cachedController == null)
                {
                    if (_animatorCache.runtimeAnimatorController != null)
                        _cachedController = (AnimatorController)_animatorCache.runtimeAnimatorController;
                    else
                    {
                        string path = Directory.GetFiles(_animatorControllerSource.stringValue, "*.controller")[0];
                        _cachedController = AssetDatabase.LoadAssetAtPath<AnimatorController>(path);
                        _animatorCache.runtimeAnimatorController = _cachedController;
                    }
                }

                List<AnimationClip> clips = new List<AnimationClip>();
                List<string> dropDown = new List<string>() { "None" };
                AnimatorStateMachine rootState = _cachedController.layers[0].stateMachine;

                foreach (string path in Directory.EnumerateFiles(_clipPath.stringValue, "*.anim"))
                    clips.Add(AssetDatabase.LoadAssetAtPath<AnimationClip>(path));
                dropDown.AddRange(clips.Select(x => x.name));

                BeginHorizontal();

                _selection.intValue = DropdownList("Default Animator State ", _selection.intValue, dropDown.ToArray());
                if (Button("Update Controller"))
                {
                    foreach (var _animatorState in rootState.states)
                        rootState.RemoveState(_animatorState.state);
                    foreach (AnimationClip clip in clips)
                    {
                        if (dropDown[_selection.intValue] == clip.name)
                        {
                            rootState.defaultState = rootState.AddState(clip.name);
                            rootState.defaultState.motion = clip;
                        }
                        else
                            rootState.AddState(clip.name).motion = clip;
                    }
                    if (dropDown[_selection.intValue] == "None")
                        rootState.defaultState = rootState.AddState("None");
                }

                EndHorizontal();
            }


        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            InpectorUpdate();
            Space(15);
            serializedObject.ApplyModifiedProperties();
        }

        private string trimPath(string pathStr)
        {
            if (string.IsNullOrEmpty(pathStr))
                return null;

            var s = pathStr.Split('/');
            int startIndex = 0;

            for (int i = 0; i < s.Length; i++)
            {
                if (s[i] == "Assets")
                    break;
                startIndex += s[i].Length + 1;
            }

            return pathStr.Substring(startIndex);
        }
    }

    public class EditorUtils<TType> : Editor where TType : Object
    {
        public TType Root => (TType)target;

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            CustomOnGUI();
            serializedObject.ApplyModifiedProperties();
        }

        public virtual void CustomOnGUI() { }

        public SerializedProperty GetProperty(string propertyName)
            => serializedObject.FindProperty(propertyName);

        public void PropertyField(SerializedProperty property)
            => PropertyField(property, "", "");

        public void PropertyField(SerializedProperty property, string propertyName, string tooltip)
            => EditorGUILayout.PropertyField(property, new GUIContent(propertyName, tooltip));

        public void Info(string info, MessageType type = MessageType.Info)
            => EditorGUILayout.HelpBox(info, type);

        public void PropertySlider(SerializedProperty property, float min, float max, string label)
            => EditorGUILayout.Slider(property, min, max, label);

        public void Space(float val)
            => GUILayout.Space(val);

        public void Heading(string label)
        {
            var style = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Bold
            };
            EditorGUILayout.LabelField(label, style, GUILayout.ExpandWidth(true));
        }
        public bool Button(string content)
            => GUILayout.Button(content);

        public bool Button(string content, float height)
            => GUILayout.Button(content, GUILayout.Height(height));

        public bool Button(string content, float height, float width)
            => GUILayout.Button(content, GUILayout.Height(height), GUILayout.Width(width));

        public int DropdownList(string label, int index, string[] choices)
            => EditorGUILayout.Popup(label, index, choices);

        public void BeginVertical()
            => EditorGUILayout.BeginVertical();

        public void EndVertical()
            => EditorGUILayout.EndVertical();

        public void BeginHorizontal()
            => EditorGUILayout.BeginHorizontal();

        public void EndHorizontal()
            => EditorGUILayout.EndHorizontal();

        public void Label(string labelContent)
            => EditorGUILayout.LabelField(labelContent);
    }

}
