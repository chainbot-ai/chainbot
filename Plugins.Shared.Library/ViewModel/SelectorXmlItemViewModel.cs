using GalaSoft.MvvmLight;
using System.Collections.ObjectModel;
using System.Linq;

namespace Plugins.Shared.Library.ViewModel
{
    /// <summary>
    /// This class contains properties that a View can data bind to.
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class SelectorXmlItemViewModel : ViewModelBase
    {
        public enum XmlType
        {
            XmlElement,
            XmlText,
            XmlAttribute
        }


        private SelectorEditorViewModel selectorEditorViewModel;

        public SelectorXmlItemViewModel Parent { get; set; }

       
        public XmlType Type { get; set; }

        public SelectorXmlItemViewModel(SelectorEditorViewModel selectorEditorViewModel, XmlType xmlType)
        {
            this.selectorEditorViewModel = selectorEditorViewModel;
            this.Type = xmlType;
        }


        /// <summary>
        /// The <see cref="Children" /> property's name.
        /// </summary>
        public const string ChildrenPropertyName = "Children";

        private ObservableCollection<SelectorXmlItemViewModel> _childrenProperty = new ObservableCollection<SelectorXmlItemViewModel>();

        /// <summary>
        /// Sets and gets the Children property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public ObservableCollection<SelectorXmlItemViewModel> Children
        {
            get
            {
                return _childrenProperty;
            }

            set
            {
                if (_childrenProperty == value)
                {
                    return;
                }

                _childrenProperty = value;
                RaisePropertyChanged(ChildrenPropertyName);
            }
        }


        /// <summary>
        /// The <see cref="Name" /> property's name.
        /// </summary>
        public const string NamePropertyName = "Name";

        private string _nameProperty = "";

        /// <summary>
        /// Sets and gets the Name property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string Name
        {
            get
            {
                return _nameProperty;
            }

            set
            {
                if (_nameProperty == value)
                {
                    return;
                }

                _nameProperty = value;
                RaisePropertyChanged(NamePropertyName);

                selectorEditorViewModel.RefershXmlEditorDocument();
            }
        }



  

        /// <summary>
        /// The <see cref="Value" /> property's name.
        /// </summary>
        public const string ValuePropertyName = "Value";

        private string _valueProperty = "";

        /// <summary>
        /// Sets and gets the Value property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string Value
        {
            get
            {
                return _valueProperty;
            }

            set
            {
                if (_valueProperty == value)
                {
                    return;
                }

                _valueProperty = value;
                RaisePropertyChanged(ValuePropertyName);

                selectorEditorViewModel.RefershXmlEditorDocument();
            }
        }



        #region IsChecked

        bool? _isChecked = true;

        public bool? IsChecked
        {
            get
            {
                return _isChecked;
            }

            set
            {
                //this.SetIsChecked(value, true, true);
                this.SetIsChecked(value, true, false);

                selectorEditorViewModel.RefershXmlEditorDocument();
            }
        }

        void SetIsChecked(bool? value, bool updateChildren, bool updateParent)
        {
            if (value == _isChecked)
                return;

            _isChecked = value;

            if (updateChildren && _isChecked.HasValue)
            {
                foreach(var item in this.Children)
                {
                    item.SetIsChecked(_isChecked, true, false);
                }
            }

            if (updateParent && Parent != null)
                Parent.VerifyCheckState();

            RaisePropertyChanged("IsChecked");
        }

        void VerifyCheckState()
        {
            bool? state = null;
            for (int i = 0; i < this.Children.Count; ++i)
            {
                bool? current = this.Children[i].IsChecked;
                if (i == 0)
                {
                    state = current;
                }
                else if (state != current)
                {
                    state = null;
                    break;
                }
            }
            this.SetIsChecked(state, false, true);
        }

        #endregion // IsChecked



    }
}