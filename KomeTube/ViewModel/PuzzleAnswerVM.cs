using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;

using KomeTube.Common;
namespace KomeTube.ViewModel
{
    public class PuzzleAnswerVM : ViewModelBase
    {
        #region Private Member
        private String _answer;
        private String _hideAnserText;
        private int _wordLength;
        private bool _isHighLight;
        private bool _isShowAnswer;
        private Dictionary<String, CommentVM> _nameTable;
        private ObservableCollection<CommentVM> _nameColle;
        private object _lockNameColleObj;
        #endregion


        #region Constructor
        public PuzzleAnswerVM()
        {
            _nameTable = new Dictionary<string, CommentVM>();
            _lockNameColleObj = new object();
            _nameColle = new ObservableCollection<CommentVM>();
            this.NameColle = CollectionViewSource.GetDefaultView(_nameColle);
            BindingOperations.EnableCollectionSynchronization(_nameColle, _lockNameColleObj);
        }
        #endregion


        #region Public Member
        /// <summary>
        /// 取得或設定答案
        /// </summary>
        public String Answer
        {
            get { return _answer; }
            set
            {
                _answer = value;
                OnPropertyChanged(nameof(Answer));
                OnPropertyChanged(nameof(HideAnserText));
                OnPropertyChanged(nameof(WordLength));
            }
        }

        /// <summary>
        /// 取得或設定以特殊符號隱藏的答案
        /// </summary>
        public String HideAnserText
        {
            get
            {
                _hideAnserText = "";
                for (int i = 0; i < this.WordLength; i++)
                {
                    _hideAnserText += "●";
                }
                return _hideAnserText;
            }
            set { _hideAnserText = value; OnPropertyChanged(nameof(HideAnserText)); }
        }

        /// <summary>
        /// 取得答案字數
        /// </summary>
        public int WordLength
        {
            get
            {
                _wordLength = _answer.Length;
                return _wordLength;
            }
        }

        /// <summary>
        /// 取得或設定是否顯示高亮
        /// </summary>
        public bool IsHighLight
        {
            get { return _isHighLight; }
            set { _isHighLight = value; OnPropertyChanged(nameof(IsHighLight)); }
        }

        /// <summary>
        /// 取得或設定是否顯示答案
        /// </summary>
        public bool IsShowAnswer
        {
            get { return _isShowAnswer; }
            set { _isShowAnswer = value; OnPropertyChanged(nameof(IsShowAnswer)); }
        }

        /// <summary>
        /// 記名名單
        /// </summary>
        public ICollectionView NameColle
        {
            get;
            private set;
        }
        #endregion


        #region Command
        public const string CmdKey_Remove = "CmdKey_Remove";
        private CommandBase _cmdRemove;
        public CommandBase CmdRemove
        {
            get
            {
                return _cmdRemove ?? (_cmdRemove = new CommandBase(x => ExecuteCommand(CmdKey_Remove)));
            }
        }

        public const string CmdKey_OpenNameColle = "CmdKey_OpenNameColle";
        private CommandBase _cmdOpenNameColle;
        public CommandBase CmdOpenNameColle
        {
            get
            {
                return _cmdOpenNameColle ?? (_cmdOpenNameColle = new CommandBase(x => ExecuteCommand(CmdKey_OpenNameColle)));
            }
        }


        public Func<String, PuzzleAnswerVM, bool> CommandAction;

        private void ExecuteCommand(String cmd)
        {
            if (this.CommandAction != null)
            {
                this.CommandAction(cmd, this);
            }
        }
        #endregion


        #region Public Method
        /// <summary>
        /// 將留言者加入記名名單中
        /// </summary>
        /// <param name="name">留言</param>
        public void AddName(CommentVM name)
        {
            _nameColle.Add(name);
            if (!_nameTable.ContainsKey(name.AuthorID))
            {
                _nameTable.Add(name.AuthorID, name);
            }
            this.IsHighLight = true;
        }

        /// <summary>
        /// 判斷留言者是否已存在
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public bool IsNameExist(CommentVM name)
        {
            return _nameTable.ContainsKey(name.AuthorID);
        }

        /// <summary>
        /// 清除統計資料
        /// </summary>
        public void Clear()
        {
            _nameColle.Clear();
            _nameTable.Clear();
            this.IsHighLight = false;
        }
        #endregion
    }
}
