using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KomeTube.ViewModel;

namespace KomeTube.Model
{
    public class VoterTable
    {
        #region Private Member

        private Dictionary<string, Dictionary<CommentVM, VoteCandidateVM>> _dataMap;

        #endregion Private Member

        #region Constructor

        public VoterTable()
        {
            _dataMap = new Dictionary<string, Dictionary<CommentVM, VoteCandidateVM>>();
        }

        #endregion Constructor

        #region Public Method

        public void Add(CommentVM voter, VoteCandidateVM candidate)
        {
            if (!_dataMap.ContainsKey(voter.AuthorID))
            {
                Dictionary<CommentVM, VoteCandidateVM> first = new Dictionary<CommentVM, VoteCandidateVM>();

                _dataMap.Add(voter.AuthorID, first);
            }

            _dataMap[voter.AuthorID].Add(voter, candidate);
        }

        public bool IsVoted(string id)
        {
            return _dataMap.ContainsKey(id);
        }

        public int GetVoterCount(string id)
        {
            if (this.IsVoted(id))
            {
                int count = _dataMap[id].Count;
                return count;
            }
            return 0;
        }

        public Dictionary<CommentVM, VoteCandidateVM> GetVoterTicket(string id)
        {
            if (this.IsVoted(id))
            {
                return _dataMap[id];
            }

            return null;
        }

        public void Clear()
        {
            _dataMap.Clear();
        }

        #endregion Public Method
    }
}