using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FlatEarth
{
    public class SelectAnimal : MonoBehaviour
    {
        private Image _image;
        private Entity _selectedAnimal;
        private Camera _camera;
        private WorldGrid _worldGrid;
        private TextMeshProUGUI _textMesh;

        public void Init(WorldGrid worldGrid)
        {
            _image = GetComponentInChildren<Image>();
            _textMesh = GetComponentInChildren<TextMeshProUGUI>();
            _camera = Camera.main;
            _worldGrid = worldGrid;
    }

    public void UpdateUI()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Node node = FindNodeFromMousePosition(_worldGrid);
            if (node == null)
            {
                return;
            }
            
            var entitiesOnNode = node.GetEntities();

            if (entitiesOnNode.Count > 0)
            {
                _selectedAnimal = entitiesOnNode[entitiesOnNode.Count-1]; 
            }
        }
        
        if (_selectedAnimal != null)
        {
            _image.enabled = true;
            var sprite = _selectedAnimal.GetCurrentState();
            if (sprite != null)
            {
                _image.sprite = _selectedAnimal.GetCurrentState();
            }
            else
            {
                _image.enabled = false;
            }
            _textMesh.text = _selectedAnimal.gameObject.name;
        }
        else
        {
            _image.enabled = false;
            _textMesh.text = "";
        }
    }

    private Node FindNodeFromMousePosition(WorldGrid worldGrid)
    {
        Node retVal = null;

        Ray ray = _camera.ScreenPointToRay(Input.mousePosition);
        RaycastHit[] hits = Physics.RaycastAll(ray, 1000);

        List<Node> groundNodes = new List<Node>();

        //Sorted the raycast hits, now it will return the closest ground node from the camera
        //did this because RaycastAll doesnt have a certain order of hits so sorting them makes for more accurate results

        foreach (var t in hits)
        {
            Node n = worldGrid.GetNodeFromWorldPos(t.point); // go find the node for each hit

            if (n != null)
            {
                groundNodes.Add(n);
            }
        }

        float minDis = Mathf.Infinity;
        foreach (var t in groundNodes)
        {
            float tmpDis = Vector3.Distance(t.GetNodeWorldPos(),
                Camera.main.transform.position);

            if (tmpDis < minDis)
            {
                minDis = tmpDis;
                retVal = t;
            }
        }
        return retVal;
    }
}
}