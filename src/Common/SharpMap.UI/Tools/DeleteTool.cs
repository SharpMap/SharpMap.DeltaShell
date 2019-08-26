using System.Linq;
using System.Windows.Forms;
using DelftTools.Utils.Editing;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Properties;

namespace SharpMap.UI.Tools
{
    public class DeleteTool : MapTool
    {
        public DeleteTool()
        {
            Name = "Delete";
        }

        public override bool AlwaysActive
        {
            get { return true; }
        }

        public override void OnKeyDown(KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Delete)
            {
                return;
            }

            if (MessageBox.Show(Properties.Resources.AskDeleteSelectionText, Properties.Resources.AskDeleteSelectionCaption,
                                MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) != DialogResult.Yes)
                return;

            DeleteSelection();
        }

        public override void OnBeforeContextMenu(ContextMenuStrip menu, Coordinate worldPosition)
        {
            var deleteMenuItem = new ToolStripMenuItem("Delete", null, delegate { DeleteSelection(); })
            {
                Enabled = MapControl.SelectTool.SelectedFeatureInteractors.Any(i => i.AllowDeletion())
            };

            menu.Items.Add(deleteMenuItem);

            base.OnBeforeContextMenu(menu, worldPosition);
        }

        public void DeleteSelection()
        {
            if (!MapControl.SelectTool.SelectedFeatureInteractors.Any(i => i.AllowDeletion()))
            {
                return;
            }

            bool featuresDeleted = false;
            var editableObject = MapControl.SelectTool.SelectedFeatureInteractors[0].EditableObject;

            var interactors = MapControl.SelectTool.SelectedFeatureInteractors.Where(featureMutator => featureMutator.AllowDeletion()).ToArray();
            foreach (var interactor in interactors)
            {
                if (!featuresDeleted && editableObject != null)
                {
                    editableObject.BeginEdit("Delete feature(s)");
                }

                interactor.Delete();
                featuresDeleted = true;
            }

            if (!featuresDeleted) return;

            if (editableObject != null)
            {
                editableObject.EndEdit();
            }

            MapControl.SelectTool.Clear();
        }

        public override bool IsBusy
        {
            get { return false; }
        }
    }
}
