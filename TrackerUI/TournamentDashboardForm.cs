using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TrackerLibrary;
using TrackerLibrary.Models;

namespace TrackerUI
{
    public partial class TournamentDashboardForm : Form , ITournamentRequester
    {
        List<TournamentModel> tournamentsAll = GlobalConfig.Connection.GetTournament_All();
        public TournamentDashboardForm()
        {
            InitializeComponent();

            InitializeLists();
        }
        private void createTournamentButton_Click(object sender, EventArgs e)
        {
            CreateTournamentForm createTournamentForm = new(this);
            createTournamentForm.Show();
            //tournamentsAll = GlobalConfig.Connection.GetTournament_All();
            //InitializeLists();
        }

        private void loadTournamentButton_Click(object sender, EventArgs e)
        {
            //TournamentModel tm = (TournamentModel)loadExistingTournamentDropdown.SelectedItem;
            if(loadExistingTournamentDropdown.SelectedItem != null)
            {
                //this.Close();
                TournamentViewerForm frm = new((TournamentModel)loadExistingTournamentDropdown.SelectedItem);
                frm.Show();
            }
            else
            {
                MessageBox.Show("Error");
            }
        }

        private void InitializeLists()
        {

            loadExistingTournamentDropdown.DataSource = null;
            loadExistingTournamentDropdown.DataSource = tournamentsAll;
            loadExistingTournamentDropdown.DisplayMember = "TournamentName";
        }

        public void TournamentComplete(TournamentModel model)
        {
            // Put this tournaments into the list
            if (model != null)
            {
                tournamentsAll.Add(model);
                InitializeLists();
            }
        }
    }
}
