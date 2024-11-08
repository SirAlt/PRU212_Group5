using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace UnityEditor.Tilemaps
{
    /// <summary>
    /// Use this to move tiles between different Tilemaps
    /// </summary>
    [CustomGridBrush(true, false, false, "Change ContactType Brush")]
    public class ChangeLayerBrush : GridBrush
    {
        public Tilemap moveTo;
        public bool floodFillSameTilesOnly = true;

        public override void Select(GridLayout gridLayout, GameObject brushTarget, BoundsInt position)
        {
            
        }

        public override void Move(GridLayout gridLayout, GameObject brushTarget, BoundsInt from, BoundsInt to)
        {

        }

        public override void MoveStart(GridLayout gridLayout, GameObject brushTarget, BoundsInt position)
        {

        }

        public override void MoveEnd(GridLayout gridLayout, GameObject brushTarget, BoundsInt position)
        {

        }

        public override void Paint(GridLayout gridLayout, GameObject brushTarget, Vector3Int position)
        {
            if (!moveTo)
                return;

            var sourceMap = brushTarget.GetComponent<Tilemap>();
            if (!sourceMap)
                return;

            var tile = sourceMap.GetTile(position);
            if (tile)
            {
                moveTo.SetTile(position, tile);
                sourceMap.SetTile(position, null);
            }
        }

        public override void Erase(GridLayout gridLayout, GameObject brushTarget, Vector3Int position)
        {

        }

        public override void Pick(GridLayout gridLayout, GameObject brushTarget, BoundsInt position, Vector3Int pickStart)
        {

        }

        public override void FloodFill(GridLayout gridLayout, GameObject brushTarget, Vector3Int position)
        {
            if (!moveTo)
                return;

            var sourceMap = brushTarget.GetComponent<Tilemap>();
            if (!sourceMap)
                return;

            var srcTile = sourceMap.GetTile(position);
            if (!srcTile)
                return;

            var field = typeof(GridBrush).GetField("m_FloodFillContiguousOnly", BindingFlags.NonPublic | BindingFlags.Instance);
            bool contiguous = (bool)field.GetValue(this);

            if(!contiguous)
            {
                foreach(Vector3Int pos in sourceMap.cellBounds.allPositionsWithin) {
                    var tile = sourceMap.GetTile(pos);
                    if (!tile)
                        continue;
                    if (!floodFillSameTilesOnly || tile == srcTile)
                    {
                        moveTo.SetTile(pos, tile);
                        sourceMap.SetTile(pos, null);
                    }
                }
            }
            else
            {
                DoFloodFill(sourceMap, srcTile, position);
            }
        }

        private void DoFloodFill(Tilemap sourceMap, TileBase srcTile, Vector3Int position)
        {
            var positions = new Stack<Vector3Int>();
            positions.Push(position);

            while (positions.Count > 0) {
                position = positions.Pop();
                var tile = sourceMap.GetTile(position);
                if (!tile)
                    continue;
                if (floodFillSameTilesOnly && tile != srcTile)
                    continue;
                moveTo.SetTile(position, tile);
                sourceMap.SetTile(position, null);

                positions.Push(new Vector3Int(position.x - 1, position.y, position.z));
                positions.Push(new Vector3Int(position.x + 1, position.y, position.z));
                positions.Push(new Vector3Int(position.x, position.y - 1, position.z));
                positions.Push(new Vector3Int(position.x, position.y + 1, position.z));
            }
        }

        public override void BoxFill(GridLayout gridLayout, GameObject brushTarget, BoundsInt position)
        {
            if (!moveTo)
                return;

            var sourceMap = brushTarget.GetComponent<Tilemap>();
            if (!sourceMap)
                return;

            foreach (Vector3Int pos in position.allPositionsWithin)
            {
                var tile = sourceMap.GetTile(pos);
                if (tile)
                {
                    moveTo.SetTile(pos, tile);
                    sourceMap.SetTile(pos, null);
                }
            }
        }

        public override void Flip(FlipAxis flip, GridLayout.CellLayout layout)
        {

        }

        public override void Rotate(RotationDirection direction, GridLayout.CellLayout layout)
        {

        }

        public override void ChangeZPosition(int change)
        {
           
        }

        public override void ResetZPosition()
        {

        }
    }

    /// <summary>
    /// The Brush Editor for a Change ContactType Brush.
    /// </summary>
    [CustomEditor(typeof(ChangeLayerBrush))]
    public class ChangeLayerBrushEditor : GridBrushEditor
    {
        public ChangeLayerBrush changeLayerBrush
        {
            get
            {
                return target as ChangeLayerBrush;
            }
        }

    }
}