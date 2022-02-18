using TrackerLibrary;
using TrackerLibrary.Models;

namespace TrackerUI
{
    public partial class TournamentViewerForm : Form
    {
        private TournamentModel tournament;
        private int[] roundNumbers;
        //private int selectedRound = 1;
        private List<MatchupModel> selectedMatchups = new();
        private MatchupModel selectedMatchup = new();
        private bool unplayedOnly = false;
        public TournamentViewerForm(TournamentModel tournament)
        {
            InitializeComponent();

            this.tournament = tournament;

            InitRoundNumbers();

            LoadFormData();

            InitializeLists();
        }

        private void LoadFormData()
        {
            tournamentName.Text = tournament.TournamentName;

            //roundDropdown.DataSource = null;
            roundDropdown.DataSource = roundNumbers;
        }

        private void InitRoundNumbers()
        {
            roundNumbers = new int[tournament.Rounds.Count()];
            for (int i = 0; i < roundNumbers.Length; i++)
            {
                roundNumbers[i] = i+1;
            }
        }

        private void InitializeLists()
        {
            matchupListbox.DataSource = null;
            matchupListbox.DataSource = selectedMatchups;
            matchupListbox.DisplayMember = "DisplayName";
        }

        private void roundDropdown_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadMatchups_All();

            InitializeLists();
        }

        private void LoadMatchups_All()
        {
            int round = (int)roundDropdown.SelectedItem;

            foreach (var matchups in tournament.Rounds)
            {
                if(matchups.First().MatchupRound == round)
                {
                    selectedMatchups = matchups;
                }
            }
        }

        private void LoadMatchups_UnplayedOnly()
        {
            var matchupsNew = new List<MatchupModel>();

            foreach (var matchup in selectedMatchups)
            {
                if (matchup.Winner == null)
                {
                    matchupsNew.Add(matchup);
                }
            }

            selectedMatchups = matchupsNew;
        }

        private void matchupListbox_SelectedIndexChanged(object sender, EventArgs e)
        {
            InitTeamNames();
        }

        private void InitTeamNames()
        {
            selectedMatchup = (MatchupModel)matchupListbox.SelectedItem;

            if(selectedMatchup != null && selectedMatchup.Entries[0].TeamCompeting != null)
            {
                if (selectedMatchup.Entries.Count == 2)
                {
                    teamOneName.Text = selectedMatchup.Entries[0].TeamCompeting.TeamName;
                    teamOneScoreValue.Text = selectedMatchup.Entries[1].Score.ToString();

                    teamTwoName.Text = selectedMatchup.Entries[1].TeamCompeting.TeamName;
                    teamTwoScoreValue.Text = selectedMatchup.Entries[0].Score.ToString();

                    EnableScore();
                }
                else
                {
                    teamOneName.Text = selectedMatchup.Entries[0].TeamCompeting.TeamName;
                    teamOneScoreValue.Text = "";

                    teamTwoName.Text = "Bye";
                    teamTwoScoreValue.Text = "";

                    DisableScore();
                }
            }
            else
            {
                teamOneName.Text = "NA";
                teamOneScoreValue.Text = "";

                teamTwoName.Text = "NA";
                teamTwoScoreValue.Text = "";

                DisableScore();
            }
        }

        private void DisableScore()
        {
            teamOneScoreValue.Enabled = false;
            teamTwoScoreValue.Enabled = false;
            scoreButton.Enabled = false;
        }

        private void EnableScore()
        {
            teamOneScoreValue.Enabled = true;
            teamTwoScoreValue.Enabled = true;
            scoreButton.Enabled = true;
        }

        private void scoreButton_Click(object sender, EventArgs e)
        {
            if (ValidateScore() && selectedMatchup != null)
            {
                selectedMatchup.Entries[1].Score = double.Parse(teamOneScoreValue.Text);
                selectedMatchup.Entries[0].Score = double.Parse(teamTwoScoreValue.Text);

                if(selectedMatchup.Entries[0].Score != selectedMatchup.Entries[1].Score)
                {
                    GetMatchupWinner();
                    //selectedMatchup.DetermineMatchupWinner();

                    foreach (var round in tournament.Rounds)
                    {
                        foreach (var rm in round)
                        {
                            foreach (var me in rm.Entries)
                            {
                               if(me.ParentMatchup != null)
                                {
                                    if (me.ParentMatchup.Id == selectedMatchup.Id)
                                    {
                                        me.TeamCompeting = selectedMatchup.Winner;
                                        GlobalConfig.Connection.UpdateMatchup(rm);
                                    }
                                }
                            }
                        }
                    }

                    // save team scores and winner to database\text file etc
                    GlobalConfig.Connection.UpdateMatchup(selectedMatchup);

                    InitializeLists();

                    MessageBox.Show("Scored");
                }
                else
                {
                    // scores are equal
                    MessageBox.Show("Error, scores have to be not equal");
                }
            }
            else
            {
                MessageBox.Show("Wrong input");
            }
        }

        private bool ValidateScore()
        {
            bool output = true;

            double teamOneScoreNumber = 0;
            bool teamOneScoreValid = double.TryParse(teamOneScoreValue.Text, out teamOneScoreNumber);

            if (!teamOneScoreValid)
            {
                output = false;
            }

            double teamTwoScoreNumber = 0;
            bool teamTwoScoreValid = double.TryParse(teamTwoScoreValue.Text, out teamTwoScoreNumber);

            if (!teamTwoScoreValid)
            {
                output = false;
            }

            return output;
        }

        private void GetMatchupWinner()
        {
            if(selectedMatchup.Entries[0].Score > selectedMatchup.Entries[1].Score)
            {
                selectedMatchup.Winner = selectedMatchup.Entries[1].TeamCompeting;
                selectedMatchup.WinnerId = selectedMatchup.Entries[1].TeamCompetingId;
            }
            else
            {
                selectedMatchup.Winner = selectedMatchup.Entries[0].TeamCompeting;
                selectedMatchup.WinnerId = selectedMatchup.Entries[0].TeamCompetingId;
            }

        }

        //private void DetermineMatchupWinner(this MatchupModel matchup)
        //{
        //    if(matchup.Entries.Count == 2)
        //    {
        //        if (matchup.Entries[0].Score > matchup.Entries[1].Score)
        //        {
        //            matchup.Winner = matchup.Entries[1].TeamCompeting;
        //            matchup.WinnerId = matchup.Entries[1].TeamCompetingId;
        //        }
        //        else
        //        {
        //            matchup.Winner = matchup.Entries[0].TeamCompeting;
        //            matchup.WinnerId = matchup.Entries[0].TeamCompetingId;
        //        }
        //    }
        //}

        private void unplayedOnlyCheckbox_CheckedChanged(object sender, EventArgs e)
        {
            unplayedOnly = unplayedOnlyCheckbox.Checked;

            if (unplayedOnly)
            {
                LoadMatchups_UnplayedOnly();
            }
            else
            {
                LoadMatchups_All();
            }

            InitializeLists();
        }

    }
}