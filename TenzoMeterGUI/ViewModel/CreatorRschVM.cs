using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using tEngine.DataModel;
using tEngine.MVVM;
using tEngine.TMeter.DataModel;

namespace TenzoMeterGUI.ViewModel {
    public class CreatorRschVM : Observed<CreatorRschVM> {
        public Action Close;
        public Action<Rsch> FixResult;
        private string mComment;
        private string mTitle;
        public Command CMDCancel { get; private set; }
        public Command CMDCreate { get; private set; }

        public string Comment {
            get { return mComment; }
            set {
                mComment = value;
                NotifyPropertyChanged( m => m.Comment );
            }
        }

        public string Title {
            get { return mTitle; }
            set {
                mTitle = value;
                NotifyPropertyChanged( m => m.Title );
            }
        }

        public CreatorRschVM() {
            Init();
        }

        public CreatorRschVM( string title ) {
            Init( title );
        }

        public void Init( string title = "" ) {
            Title = title;
            CMDCreate = new Command( Create );
            CMDCancel = new Command( Cancel );
        }

        private void Cancel() {
            EndDialog();
        }

        private void Create() {
            EndDialog( new Rsch() {
                Comment = Comment,
                Title = Title,
                CreateTime = DateTime.Now
            } );
        }

        private void EndDialog( Rsch result = null ) {
            if( FixResult != null )
                FixResult( result );
            if( Close != null )
                Close();
        }
    }
}