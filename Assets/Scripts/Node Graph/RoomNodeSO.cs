using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

public class RoomNodeSO : ScriptableObject
{
    public string id;
    public List<string> parentRoomNodeIds = new List<string>();
    public List<string> childRoomNodeIds = new List<string>();
    [HideInInspector] public RoomNodeGraphSO roomNodeGraph;
    [HideInInspector] public RoomNodeTypeListSO roomNodeTypeList;

    public RoomNodeTypeSO roomNodeType;

    #region Editor Code

#if UNITY_EDITOR
    [HideInInspector] public Rect rect;
    [HideInInspector] public bool isLeftClickDragging;
    [HideInInspector] public bool isSelected = false;

    public void Initialise(Rect rect, RoomNodeGraphSO roomNodeGraph, RoomNodeTypeSO roomNodeType)
    {
        this.rect = rect;
        this.id = Guid.NewGuid().ToString();
        this.name = "RoomNode";
        this.roomNodeGraph = roomNodeGraph;
        this.roomNodeType = roomNodeType;

        roomNodeTypeList = GameResources.Instance.roomNodeTypeList;
    }

    public void Draw(GUIStyle nodeStyle)
    {
        GUILayout.BeginArea(rect, nodeStyle);
        EditorGUI.BeginChangeCheck();

        if (parentRoomNodeIds.Count > 0 || roomNodeType.isEntrance)
        {
            EditorGUILayout.LabelField(roomNodeType.roomNodeTypeName);
        }
        else
        {
            int selected = roomNodeTypeList.list.FindIndex(x => x == roomNodeType);
            int selection = EditorGUILayout.Popup("", selected, GetRoomNodeTypesToDisplay());

            roomNodeType = roomNodeTypeList.list[selection];

            var selectedRoomType = roomNodeTypeList.list[selected];

            if (selectedRoomType.isCorridor && !roomNodeType.isCorridor || !selectedRoomType.isCorridor && roomNodeType.isCorridor ||
                !selectedRoomType.isBossRoom && roomNodeType.isBossRoom)
            {
                if (childRoomNodeIds.Count > 0)
                {
                    for (int i = childRoomNodeIds.Count - 1; i >= 0; i--)
                    {
                        var childRoomNode = roomNodeGraph.GetRoomNode(childRoomNodeIds[i]);

                        if (childRoomNode != null)
                        {
                            RemoveChildID(childRoomNode.id);
                            childRoomNode.RemoveParentID(id);
                        }
                    }
                }
            }
        }

        if (EditorGUI.EndChangeCheck())
            EditorUtility.SetDirty(this);

        GUILayout.EndArea();
    }

    public string[] GetRoomNodeTypesToDisplay()
    {
        string[] roomTypes = new string[roomNodeTypeList.list.Count];

        for (int i = 0; i < roomNodeTypeList.list.Count; i++)
        {
            if (roomNodeTypeList.list[i].displayInNodeGraphEditor)
            {
                roomTypes[i] = roomNodeTypeList.list[i].roomNodeTypeName;
            }
        }

        return roomTypes;
    }

    public void ProcessEvents(Event currentEvent)
    {
        switch (currentEvent.type)
        {
            case EventType.MouseDown:
                ProcessMouseDownEvent(currentEvent);
                break;
            case EventType.MouseUp:
                ProcessMouseUpEvent(currentEvent);
                break;
            case EventType.MouseDrag:
                ProcessMouseDragEvent(currentEvent);
                break;
            default: break;
        }
    }

    private void ProcessMouseDownEvent(Event currentEvent)
    {
        if (currentEvent.button == 0)
        {
            ProcessLeftClickDownEvent();
        }
        else if (currentEvent.button == 1)
        {
            ProcessRightClickDownEvent(currentEvent);
        }
    }

    private void ProcessLeftClickDownEvent()
    {
        Selection.activeObject = this;

        isSelected = !isSelected;
    }

    private void ProcessRightClickDownEvent(Event currentEvent)
    {
        roomNodeGraph.SetNodeToDrawConnectionLineFrom(this, currentEvent.mousePosition);
    }

    private void ProcessMouseUpEvent(Event currentEvent)
    {
        if (currentEvent.button == 0)
        {
            ProcessLeftClickUpEvent();
        }
    }

    private void ProcessLeftClickUpEvent()
    {
        if (isLeftClickDragging)
        {
            isLeftClickDragging = false;
        }
    }

    private void ProcessMouseDragEvent(Event currentEvent)
    {
        if (currentEvent.button == 0)
        {
            ProcessLeftMouseDragEvent(currentEvent);
        }
    }

    private void ProcessLeftMouseDragEvent(Event currentEvent)
    {
        isLeftClickDragging = true;

        DragNode(currentEvent.delta);
        GUI.changed = true;
    }

    public void DragNode(Vector2 delta)
    {
        rect.position += delta;
        EditorUtility.SetDirty(this);
    }

    public bool AddChildRoomNodeID(string childID)
    {
        if (IsChildRoomValid(childID))
        {
            childRoomNodeIds.Add(childID);
            return true;
        }

        return false;
    }

    private bool IsChildRoomValid(string childID)
    {
        bool bossNodeExists = false;

        foreach (var roomNode in roomNodeGraph.roomNodeList)
        {
            if (roomNode.roomNodeType.isBossRoom && roomNode.parentRoomNodeIds.Count > 0)
            {
                bossNodeExists = true;
                break;
            }
        }

        if (roomNodeGraph.GetRoomNode(childID).roomNodeType.isBossRoom && bossNodeExists)
            return false;

        if (roomNodeGraph.GetRoomNode(childID).roomNodeType.isNone)
            return false;

        if (childRoomNodeIds.Contains(childID))
            return false;

        if (id.Equals(childID))
            return false;

        if (parentRoomNodeIds.Contains(childID))
            return false;

        if (roomNodeGraph.GetRoomNode(childID).parentRoomNodeIds.Count > 0)
            return false;

        if (roomNodeGraph.GetRoomNode(childID).roomNodeType.isCorridor && roomNodeType.isCorridor)
            return false;

        if (!roomNodeGraph.GetRoomNode(childID).roomNodeType.isCorridor && !roomNodeType.isCorridor)
            return false;

        if (roomNodeGraph.GetRoomNode(childID).roomNodeType.isCorridor && childRoomNodeIds.Count >= Settings.maxChildCorridors)
            return false;

        if (roomNodeGraph.GetRoomNode(childID).roomNodeType.isEntrance)
            return false;

        if (!roomNodeGraph.GetRoomNode(childID).roomNodeType.isCorridor && childRoomNodeIds.Count > 0)
            return false;

        return true;
    }

    public bool AddParentRoomNodeId(string parentID)
    {
        parentRoomNodeIds.Add(parentID);
        return true;
    }

    public bool RemoveChildID(string childID)
    {
        if (childRoomNodeIds.Contains(childID))
        {
            childRoomNodeIds.Remove(childID);
            return true;
        }

        return false;
    }

    public bool RemoveParentID(string parentID)
    {
        if (parentRoomNodeIds.Contains(parentID))
        {
            parentRoomNodeIds.Remove(parentID);
            return true;
        }

        return false;
    }
#endif

    #endregion
}
