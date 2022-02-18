using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrackerLibrary.Models
{
    public class MatchupModel
    {
        public int Id { get; set; }

        public List<MatchupEntryModel> Entries { get; set; } = new List<MatchupEntryModel>();

        public int WinnerId { get; set; }

        public TeamModel Winner { get; set; }

        public int MatchupRound { get; set; }

        public string DisplayName
        {
            get 
            {
                string output = "";
                foreach (var me in Entries)
                {
                    if(me.TeamCompeting != null)
                    {
                        if (output == "")
                        {
                            output = me.TeamCompeting.TeamName;
                        }
                        else
                        {
                            output += $" vs. {me.TeamCompeting.TeamName}";
                        }
                    }
                    else
                    {
                        return "NA";
                        break;
                    }
                }

                if (Winner == null)
                {
                    output = output.Insert(0, "* ");
                }

                return output;
            }
        }

        //public void DetermineMatchupWinner()
        //{
        //    if(Entries != null)
        //    {
        //        if (Entries.Count == 2)
        //        {
        //            if (Entries[0].Score > Entries[1].Score)
        //            {
        //                Winner = Entries[1].TeamCompeting;
        //                WinnerId = Entries[1].TeamCompetingId;
        //            }
        //            else
        //            {
        //                Winner = Entries[0].TeamCompeting;
        //                WinnerId = Entries[0].TeamCompetingId;
        //            }
        //        }
        //    }
        //}
    }
}
