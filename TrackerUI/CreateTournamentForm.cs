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
    public partial class CreateTournamentForm : Form, IPrizeRequester, ITeamRequester
    {
        private List<TeamModel> avalableTeams = GlobalConfig.Connection.GetTeam_All();
        private List<TeamModel> selectedTeams = new();
        private List<PrizeModel> selectedPrizes = new();
        public CreateTournamentForm()
        {
            InitializeComponent();

            InitializeLists();

        }

        ITournamentRequester callingForm;
        public CreateTournamentForm(ITournamentRequester caller)
        {
            InitializeComponent();

            InitializeLists();

            callingForm = caller;
        }

        private void InitializeLists()
        {
            avalableTeams = avalableTeams.OrderBy(t => t.TeamName).ToList();
            selectTeamDropdown.DataSource = null;
            selectTeamDropdown.DataSource = avalableTeams;
            selectTeamDropdown.DisplayMember = "TeamName";

            selectedTeams = selectedTeams.OrderBy(t => t.TeamName).ToList();
            tournamentTeamsListbox.DataSource = null;
            tournamentTeamsListbox.DataSource = selectedTeams;
            tournamentTeamsListbox.DisplayMember = "TeamName";

            //selectedPrizes = selectedPrizes.OrderBy(p => p.PlaceName).ToList();
            prizesListbox.DataSource = null;
            prizesListbox.DataSource = selectedPrizes;
            prizesListbox.DisplayMember = "PlaceName";
        }

        private void addTeamButton_Click(object sender, EventArgs e)
        {
            TeamModel t = (TeamModel)selectTeamDropdown.SelectedItem;

            if (t != null)
            {
                avalableTeams.Remove(t);
                selectedTeams.Add(t);

                InitializeLists();
            }
        }

        private void deleteSelectedPlayersButton_Click(object sender, EventArgs e)
        {
            TeamModel t = (TeamModel)tournamentTeamsListbox.SelectedItem;

            if (t != null)
            {
                avalableTeams.Add(t);
                selectedTeams.Remove(t);

                InitializeLists();
            }
        }

        private void createPrizeButton_Click(object sender, EventArgs e)
        {
            // Call the CreatePrizeForm
            CreatePrizeForm frm = new(this);
            frm.Show();
        }

        public void PrizeComplete(PrizeModel model)
        {
            // Put this PrizeModel into the list
            if(model != null)
            {
                selectedPrizes.Add(model);
                InitializeLists();
            }
        }

        private void deleteSelectedPrizeButton_Click(object sender, EventArgs e)
        {
            var p = (PrizeModel)prizesListbox.SelectedItem;

            if(p != null)
            {
                selectedPrizes.Remove(p);
                InitializeLists();
            }
        }

        private void createNewTeamLink_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            CreateTeamForm frm = new(this);
            frm.Show();
        }

        public void TeamComplete(TeamModel model)
        {
            if (model != null)
            {
                selectedTeams.Add(model);
                InitializeLists();
            }
        }

        private void createTournamentButton_Click(object sender, EventArgs e)
        {
            // Validate form
            if (ValidateTournamentForm())
            {
                // Create a tournament model
                var model = new TournamentModel();
                model.TournamentName = tournamentNameValue.Text;
                model.EntryFee = decimal.Parse(entryFeeValue.Text);

                // Create prize entries
                model.Prizes = selectedPrizes;

                // Create team entries
                model.EnteredTeams = selectedTeams;

                // Create matchups
                TournamentLogic.CreateRounds(model);

                GlobalConfig.Connection.CreateTournament(model);

                MessageBox.Show("Tournament created");
                callingForm.TournamentComplete(model);
                this.Close();
                //tournamentNameValue.Text = "";
                //entryFeeValue.Text = "0";
                //avalableTeams = GlobalConfig.Connection.GetTeam_All();
                //selectedTeams = new();
                //selectedPrizes = new();
                //InitializeLists();
    }
            else
            {
                MessageBox.Show("Invalid input");
            }
            
        }

        private bool ValidateTournamentForm()
        {
            bool output = true;

            if(tournamentNameValue.Text == null)
            {
                output = false;
            }

            if (tournamentNameValue.Text == "")
            {
                output = false;
            }

            decimal fee;
            bool feeValid = decimal.TryParse(entryFeeValue.Text, out fee);

            if (!feeValid)
            {
                output = false;
            }

            if(fee < 0)
            {
                output = false;
            }

            return output;
        }
    }
}
