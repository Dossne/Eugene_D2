using UnityEditor;
using UnityEngine;

namespace Infrastructure.Configuration
{
    [CustomEditor(typeof(ListConfiguration<,>), true), CanEditMultipleObjects]
    public class ListConfigurationEditor : JsonConvertableConfigEditor
    {
        protected override void DrawComponents()
        {
            if (target is not IListConfiguration)
            {
                return;
            }
            IListConfiguration list = (IListConfiguration)target;
            if (GUILayout.Button("Sort and rename elements"))
            {
                list.SortAndRename_Editor();
            }

            base.DrawComponents();            
        }
    }
}
