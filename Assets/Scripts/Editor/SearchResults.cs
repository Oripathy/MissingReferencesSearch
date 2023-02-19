using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace Editor
{
    public class SearchResults : EditorWindow
    {
        private List<PropertyInfo> _propertyInfos;
        private MultiColumnHeaderState _multiColumnHeaderState;
        private MultiColumnHeader _multiColumnHeader;
        private MultiColumnHeaderState.Column[] _columns;
        private readonly Color _lighterColor = Color.white * 0.3f;
        private readonly Color _darkerColor = Color.white * 0.1f;
        private Vector2 _scrollPosition;
        
        public void Initialize(List<PropertyInfo> propertyInfos)
        {
            _propertyInfos = propertyInfos;
        }

        public void Clear()
        {
            _propertyInfos = default;
        }

        private void OnGUI()
        {
            if (_propertyInfos == null)
            {
                return;
            }

            if (_propertyInfos.Count == 0)
            {
                GUILayout.Label("There Is No Missing References In Assets");
                return;
            }
            
            if (_multiColumnHeader == null)
            {
                CreateTable();
            }
            
            GUILayout.FlexibleSpace();
            var windowRect = GUILayoutUtility.GetLastRect();
            windowRect.width = position.width;
            windowRect.height = position.height;
            var columnHeight = EditorGUIUtility.singleLineHeight;
            var columnRectPrototype = new Rect(source: windowRect)
            {
                height = columnHeight
            };
            
            _scrollPosition = GetScrollViewPosition(_scrollPosition, windowRect);
            _multiColumnHeader.OnGUI(rect: columnRectPrototype, xScroll: 0f);
            for (var i = 0; i < _propertyInfos.Count; i++)
            {
                DrawTablesRow(columnRectPrototype, columnHeight, i, _propertyInfos[i]);
            }
            
            GUI.EndScrollView(handleScrollWheel: true);
        }

        private Vector2 GetScrollViewPosition(Vector2 previousScrollViewPosition, Rect windowRect)
        {
            var positionalRectAreaOfScrollView = GUILayoutUtility.GetRect(0, float.MaxValue, 0, float.MaxValue);
            var viewRect = new Rect(source: windowRect)
            {
                xMax = _columns.Sum((column) => column.width)
            };
            
            return GUI.BeginScrollView(
                position: positionalRectAreaOfScrollView,
                scrollPosition: previousScrollViewPosition,
                viewRect: viewRect,
                alwaysShowHorizontal: false,
                alwaysShowVertical: false);
        }

        private void DrawTablesRow(Rect columnRectPrototype, float columnHeight, int index, PropertyInfo propertyInfo)
        {
            var rowRect = new Rect(source: columnRectPrototype);
                rowRect.y += columnHeight * (index + 1);
                var color = index % 2 == 0 ? _darkerColor : _lighterColor;
                EditorGUI.DrawRect(rect: rowRect, color: color);
                var columnIndex = 0;
                DrawLabelField(columnIndex, rowRect, propertyInfo.PropertyName);
                columnIndex = 1;
                DrawLabelField(columnIndex, rowRect, propertyInfo.ComponentName);
                columnIndex = 2;
                DrawLabelField(columnIndex, rowRect, propertyInfo.PropertyPath);
                columnIndex = 3;
                DrawObjectField(columnIndex, rowRect, propertyInfo);
        }

        private void DrawLabelField(int columnIndex, Rect rowRect, string content)
        {
            if (_multiColumnHeader.IsColumnVisible(columnIndex: columnIndex))
            {
                var visibleColumnIndex = _multiColumnHeader.GetVisibleColumnIndex(columnIndex: columnIndex);
                var columnRect = _multiColumnHeader.GetColumnRect(visibleColumnIndex: visibleColumnIndex);
                columnRect.y = rowRect.y;
                var nameFieldGUIStyle = new GUIStyle(GUI.skin.label)
                {
                    padding = new RectOffset(left: 10, right: 10, top: 2, bottom: 2)
                };
                    
                EditorGUI.LabelField(
                    position: _multiColumnHeader.GetCellRect(visibleColumnIndex: visibleColumnIndex, columnRect),
                    label: new GUIContent(content),
                    style: nameFieldGUIStyle);
            }
        }

        private void DrawObjectField(int columnIndex, Rect rowRect, PropertyInfo propertyInfo)
        {
            if (_multiColumnHeader.IsColumnVisible(columnIndex: columnIndex))
            {
                var visibleColumnIndex = _multiColumnHeader.GetVisibleColumnIndex(columnIndex: columnIndex);
                var columnRect = _multiColumnHeader.GetColumnRect(visibleColumnIndex: visibleColumnIndex);
                columnRect.y = rowRect.y;
                EditorGUI.ObjectField(
                    position: _multiColumnHeader.GetCellRect(visibleColumnIndex: visibleColumnIndex,
                        columnRect),
                    obj: propertyInfo.Object,
                    objType: typeof(Object),
                    allowSceneObjects: false);
            }
        }
        
        private void CreateTable()
        {
            _columns = new[]
            {
                CreateColumn(new GUIContent("Property Name", "Name of the property with the missing reference"), false),
                CreateColumn(
                    new GUIContent("Component Name",
                        "Name of the component that have the property with the missing reference"), true),
                CreateColumn(new GUIContent("Property Path", "Actual path to the property"), true),
                CreateColumn(
                    new GUIContent("Reference To The Object",
                        "Clickable reference to the object that holds the property with the missing reference"), true)
            };

            _multiColumnHeaderState = new MultiColumnHeaderState(_columns);
            _multiColumnHeader = new MultiColumnHeader(_multiColumnHeaderState);
            _multiColumnHeader.visibleColumnsChanged += OnVisibleColumnsChanged;
            _multiColumnHeader.ResizeToFit();
        }

        private MultiColumnHeaderState.Column CreateColumn(GUIContent headerContent, bool allowToggleVisibility)
        {
            return new MultiColumnHeaderState.Column
            {
                allowToggleVisibility = allowToggleVisibility,
                autoResize = true,
                minWidth = 300f,
                canSort = false,
                sortingArrowAlignment = TextAlignment.Right,
                headerContent = headerContent,
                headerTextAlignment = TextAlignment.Center
            };
        }

        private void OnVisibleColumnsChanged(MultiColumnHeader multiColumnHeader)
        {
            multiColumnHeader.ResizeToFit();
        }

        private void OnDestroy()
        {
            _multiColumnHeader.visibleColumnsChanged -= OnVisibleColumnsChanged;
        }
    }
}