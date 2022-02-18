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
    public partial class CreateTeamForm : Form
    {
        private List<PersonModel> avalableTeamMembers = GlobalConfig.Connection.GetPerson_All();
        private List<PersonModel> selectedTeamMembers = new();
        private ITeamRequester callingForm;

        public CreateTeamForm(ITeamRequester caller)
        {
            InitializeComponent();

            //Test1();

            InitializeLists();

            callingForm = caller;
        }

        private void Test1()
        {
            avalableTeamMembers.Add(new PersonModel { FirstName = "firstname1", LastName = "lastname1" });
            avalableTeamMembers.Add(new PersonModel { FirstName = "firstname2", LastName = "lastname2" });

            selectedTeamMembers.Add(new PersonModel { FirstName = "firstname3", LastName = "lastname3" });
            selectedTeamMembers.Add(new PersonModel { FirstName = "firstname4", LastName = "lastname4" });
        }

        // TODO - look up ways to refresh lists in WireUpLists()
        private void InitializeLists()
        {
            selectTeamMemberDropdown.DataSource = null;
            selectTeamMemberDropdown.DataSource = avalableTeamMembers;
            selectTeamMemberDropdown.DisplayMember = "FullName";

            teamMembersListbox.DataSource = null;
            teamMembersListbox.DataSource = selectedTeamMembers;
            teamMembersListbox.DisplayMember = "FullName";
        }

        private void createMemberButton_Click(object sender, EventArgs e)
        {
            if (ValidateMemberForm())
            {
                var model = new PersonModel(
                    firstNameValue.Text,
                    lastNameValue.Text,
                    emailValue.Text,
                    cellphoneValue.Text
                );

                var p = GlobalConfig.Connection.CreatePerson(model);

                selectedTeamMembers.Add(p);

                InitializeLists();

                firstNameValue.Text = "";
                lastNameValue.Text = "";
                emailValue.Text = "";
                cellphoneValue.Text = "";
                MessageBox.Show("Member created");
            }
            else
            {
                MessageBox.Show("This form has invalid input. Please try again.");
            }
        }

        private bool ValidateMemberForm()
        {
            bool output = true;

            if(firstNameValue.Text.Length == 0)
            {
                output = false;
            }

            if (lastNameValue.Text.Length == 0)
            {
                output = false;
            }

            if (emailValue.Text.Length == 0)
            {
                output = false;
            }

            if (cellphoneValue.Text.Length == 0)
            {
                output = false;
            }
            return output;
        }

        private void addMemberButton_Click(object sender, EventArgs e)
        {
            PersonModel p = (PersonModel)selectTeamMemberDropdown.SelectedItem;

            if(p != null)
            {
                avalableTeamMembers.Remove(p);
                selectedTeamMembers.Add(p);

                InitializeLists();
            }
        }

        private void removeSelectedMembersButton_Click(object sender, EventArgs e)
        {
            PersonModel p = (PersonModel)teamMembersListbox.SelectedItem;

            if(p != null)
            {
                avalableTeamMembers.Add(p);
                selectedTeamMembers.Remove(p);

                InitializeLists();
            }
        }

        private void createTeamButton_Click(object sender, EventArgs e)
        {
            var t = new TeamModel();

            if(teamNameValue.Text != null)
            {
                t.TeamName = teamNameValue.Text;
                t.TeamMembers = selectedTeamMembers;

                GlobalConfig.Connection.CreateTeam(t);
                callingForm.TeamComplete(t);

                this.Close();
                MessageBox.Show("Team created");
            }
            else
            {
                MessageBox.Show("Invalid team name");
            }
        }
    }
}
