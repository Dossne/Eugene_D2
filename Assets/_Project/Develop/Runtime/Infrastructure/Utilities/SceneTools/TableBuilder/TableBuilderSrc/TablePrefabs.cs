using UnityEngine;

namespace Infrastructure.Utilities
{
    [CreateAssetMenu(fileName = "TablePrefabs", menuName = "Config/Tools/TablePrefabs")]
    public class TablePrefabs : ScriptableObject
    {
        public TableBlock  tableBlockPf;
        public TableBorder tableBorderPf;
        public TableCorner tableCornerPf;
    }
}