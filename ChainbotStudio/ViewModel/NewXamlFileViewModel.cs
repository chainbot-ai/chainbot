using Chainbot.Contracts.Config;
using Chainbot.Contracts.Utils;
using Chainbot.Cores.Classes;
using Chainbot.Resources.Librarys;
using ChainbotStudio.ViewModel;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Xml;

namespace ChainbotStudio.ViewModel
{
    /// <summary>
    /// This class contains properties that a View can data bind to.
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class NewXamlFileViewModel : ViewModelBase
    {
        private ICommonService _commonService;
        private IConstantConfigService _constantConfigService;

        private ProjectViewModel _projectViewModel;
        private readonly Regex _disallowedCharacters = new Regex("[\\W-]");
        /// <summary>
        /// Initializes a new instance of the NewXamlFileViewModel class.
        /// </summary>
        public NewXamlFileViewModel(ICommonService commonService, IConstantConfigService constantConfigService
            , ProjectViewModel projectViewModel)
        {
            _commonService = commonService;
            _constantConfigService = constantConfigService;
            _projectViewModel = projectViewModel;
        }


        private Window _view;

        public string ProjectPath { get; internal set; }

        public enum enFileType
        {
            Null = 0,
            Sequence,
            Flowchart,
            StateMachine,
            GlobalHandler,
        }



        private RelayCommand<RoutedEventArgs> _loadedCommand;

        public RelayCommand<RoutedEventArgs> LoadedCommand
        {
            get
            {
                return _loadedCommand
                    ?? (_loadedCommand = new RelayCommand<RoutedEventArgs>(
                    p =>
                    {
                        _view = (Window)p.Source;
                    }));
            }
        }


        private RelayCommand<RoutedEventArgs> _fileNameLoadedCommand;

        public RelayCommand<RoutedEventArgs> FileNameLoadedCommand
        {
            get
            {
                return _fileNameLoadedCommand
                    ?? (_fileNameLoadedCommand = new RelayCommand<RoutedEventArgs>(
                    p =>
                    {
                        var textBox = (TextBox)p.Source;
                        textBox.Focus();
                        textBox.SelectAll();
                    }));
            }
        }



        /// <summary>
        /// The <see cref="FileType" /> property's name.
        /// </summary>
        public const string FileTypePropertyName = "FileType";

        private enFileType _fileTypeProperty = enFileType.Null;

 
        public enFileType FileType
        {
            get
            {
                return _fileTypeProperty;
            }

            set
            {
                if (_fileTypeProperty == value)
                {
                    return;
                }

                _fileTypeProperty = value;
                RaisePropertyChanged(FileTypePropertyName);

                initInfoByFileType(value);
            }
        }


        private void initInfoByFileType(enFileType value)
        {
            XmlDocument doc = new XmlDocument();

            using (var ms = new MemoryStream(ResourcesLoader.NewXamlFileConfig))
            {
                ms.Flush();
                ms.Position = 0;
                doc.Load(ms);
                ms.Close();
            }

            var rootNode = doc.DocumentElement;

            var nodeTypeStr = value.ToString();
            var processElement = rootNode.SelectSingleNode(nodeTypeStr) as XmlElement;
            Title = processElement.GetAttribute("Title");
            Description = processElement.GetAttribute("Description");
            var fileName = processElement.GetAttribute("FileName");

            var newfileNameWithExt = _commonService.GetValidFileName(FilePath, fileName + _constantConfigService.XamlFileExtension, "", "{0}", 1);
            FileName = Path.GetFileNameWithoutExtension(newfileNameWithExt);
        }



        /// <summary>
        /// The <see cref="Title" /> property's name.
        /// </summary>
        public const string TitlePropertyName = "Title";

        private string _titleProperty = "";

        public string Title
        {
            get
            {
                return _titleProperty;
            }

            set
            {
                if (_titleProperty == value)
                {
                    return;
                }

                _titleProperty = value;
                RaisePropertyChanged(TitlePropertyName);
            }
        }



        /// <summary>
        /// The <see cref="Description" /> property's name.
        /// </summary>
        public const string DescriptionPropertyName = "Description";

        private string _descriptionProperty = "";

        public string Description
        {
            get
            {
                return _descriptionProperty;
            }

            set
            {
                if (_descriptionProperty == value)
                {
                    return;
                }

                _descriptionProperty = value;
                RaisePropertyChanged(DescriptionPropertyName);
            }
        }



        /// <summary>
        /// The <see cref="IsFileNameCorrect" /> property's name.
        /// </summary>
        public const string IsFileNameCorrectPropertyName = "IsFileNameCorrect";

        private bool _isFileNameCorrectProperty = false;

        public bool IsFileNameCorrect
        {
            get
            {
                return _isFileNameCorrectProperty;
            }

            set
            {
                if (_isFileNameCorrectProperty == value)
                {
                    return;
                }

                _isFileNameCorrectProperty = value;
                RaisePropertyChanged(IsFileNameCorrectPropertyName);
            }
        }


        /// <summary>
        /// The <see cref="FileNameValidatedWrongTip" /> property's name.
        /// </summary>
        public const string FileNameValidatedWrongTipPropertyName = "FileNameValidatedWrongTip";

        private string _fileNameValidatedWrongTipProperty = "";

        public string FileNameValidatedWrongTip
        {
            get
            {
                return _fileNameValidatedWrongTipProperty;
            }

            set
            {
                if (_fileNameValidatedWrongTipProperty == value)
                {
                    return;
                }

                _fileNameValidatedWrongTipProperty = value;
                RaisePropertyChanged(FileNameValidatedWrongTipPropertyName);
            }
        }




        /// <summary>
        /// The <see cref="FileName" /> property's name.
        /// </summary>
        public const string FileNamePropertyName = "FileName";

        private string _fileNameProperty = "";

        public string FileName
        {
            get
            {
                return _fileNameProperty;
            }

            set
            {
                if (_fileNameProperty == value)
                {
                    return;
                }

                _fileNameProperty = value;
                RaisePropertyChanged(FileNamePropertyName);

                fileNameValidate(value);
            }
        }

        private void fileNameValidate(string value)
        {
            IsFileNameCorrect = true;

            if (string.IsNullOrEmpty(value))
            {
                IsFileNameCorrect = false;
                FileNameValidatedWrongTip = Chainbot.Resources.Properties.Resources.NewProject_NameValidateWrong1;
            }
            else
            {
                if (value.Contains(@"\") || value.Contains(@"/"))
                {
                    IsFileNameCorrect = false;
                    FileNameValidatedWrongTip = Chainbot.Resources.Properties.Resources.NewProject_NameValidateWrong2;
                }
                else
                {
                    System.IO.FileInfo fi = null;
                    try
                    {
                        fi = new System.IO.FileInfo(value);
                    }
                    catch (ArgumentException) { }
                    catch (System.IO.PathTooLongException) { }
                    catch (NotSupportedException) { }
                    if (ReferenceEquals(fi, null))
                    {
                        // file name is not valid
                        IsFileNameCorrect = false;
                        FileNameValidatedWrongTip = Chainbot.Resources.Properties.Resources.NewProject_NameValidateWrong2;
                    }
                    else
                    {
                        // file name is valid... May check for existence by calling fi.Exists.
                    }
                }
            }

            if (File.Exists(FilePath + @"\" + FileName + _constantConfigService.XamlFileExtension))
            {
                IsFileNameCorrect = false;
                FileNameValidatedWrongTip = Chainbot.Resources.Properties.Resources.NewFile_NameValidateWrong;
            }
        }




        /// <summary>
        /// The <see cref="IsFilePathCorrect" /> property's name.
        /// </summary>
        public const string IsFilePathCorrectPropertyName = "IsFilePathCorrect";

        private bool _isFilePathCorrectProperty = false;

        public bool IsFilePathCorrect
        {
            get
            {
                return _isFilePathCorrectProperty;
            }

            set
            {
                if (_isFilePathCorrectProperty == value)
                {
                    return;
                }

                _isFilePathCorrectProperty = value;
                RaisePropertyChanged(IsFilePathCorrectPropertyName);
            }
        }


        /// <summary>
        /// The <see cref="FilePathValidatedWrongTip" /> property's name.
        /// </summary>
        public const string FilePathValidatedWrongTipPropertyName = "FilePathValidatedWrongTip";

        private string _filePathValidatedWrongTipProperty = "";

        public string FilePathValidatedWrongTip
        {
            get
            {
                return _filePathValidatedWrongTipProperty;
            }

            set
            {
                if (_filePathValidatedWrongTipProperty == value)
                {
                    return;
                }

                _filePathValidatedWrongTipProperty = value;
                RaisePropertyChanged(FilePathValidatedWrongTipPropertyName);
            }
        }



        /// <summary>
        /// The <see cref="FilePath" /> property's name.
        /// </summary>
        public const string FilePathPropertyName = "FilePath";

        private string _filePathProperty = "";

        public string FilePath
        {
            get
            {
                return _filePathProperty;
            }

            set
            {
                if (_filePathProperty == value)
                {
                    return;
                }

                _filePathProperty = value;
                RaisePropertyChanged(FilePathPropertyName);

                filePathValidate(value);
                fileNameValidate(FileName);
            }
        }

        private void filePathValidate(string value)
        {
            IsFilePathCorrect = true;
            if (string.IsNullOrEmpty(value))
            {
                IsFilePathCorrect = false;
                FilePathValidatedWrongTip = Chainbot.Resources.Properties.Resources.NewProject_LocationValidateWrong1;
            }
            else
            {
                if (!Directory.Exists(value))
                {
                    IsFilePathCorrect = false;
                    FilePathValidatedWrongTip = Chainbot.Resources.Properties.Resources.NewProject_LocationValidateWrong2;
                }
                else if (!value.IsSubPathOf(ProjectPath))
                {
                    IsFilePathCorrect = false;
                    FilePathValidatedWrongTip = Chainbot.Resources.Properties.Resources.NewFile_PathValidateWrong;
                }
            }
        }

        private RelayCommand _selectFilePathCommand;

        public RelayCommand SelectFilePathCommand
        {
            get
            {
                return _selectFilePathCommand
                    ?? (_selectFilePathCommand = new RelayCommand(
                    () =>
                    {
                        string dst_dir = "";
                        if (_commonService.ShowSelectDirDialog(Title, ref dst_dir))
                        {
                            FilePath = dst_dir;
                        }
                    }));
            }
        }



        private RelayCommand _createFileCommand;

        public RelayCommand CreateFileCommand
        {
            get
            {
                return _createFileCommand
                    ?? (_createFileCommand = new RelayCommand(
                    () =>
                    {
                        var xamlFilePath = newXamlFile();

                        _projectViewModel.Refresh();

                        if (!string.IsNullOrEmpty(xamlFilePath))
                        {
                            var item = _projectViewModel.GetProjectItemByFullPath(xamlFilePath);
                            if (item is ProjectFileItemViewModel)
                            {
                                (item as ProjectFileItemViewModel).OpenXamlCommand.Execute(null);
                            }
                        }

                        _view.Close();
                    },
                    () => IsFileNameCorrect && IsFilePathCorrect));
            }
        }






        private string newXamlFile()
        {
            var retPath = "";
            byte[] data = null;

            var fileName = FileName.Trim();
            var title = _disallowedCharacters.Replace(fileName, "_");

            switch (FileType)
            {
                case enFileType.Sequence:
                    data = ResourcesLoader.Sequence;
                    break;
                case enFileType.Flowchart:
                    data = ResourcesLoader.Flowchart;
                    break;
                case enFileType.StateMachine:
                    data = ResourcesLoader.StateMachine;
                    break;
                case enFileType.GlobalHandler:
                    break;
            }

            if (data != null)
            {
                string str = System.Text.Encoding.UTF8.GetString(data);
                if (char.IsDigit(title[0]))
                {
                    str = str.Replace("{{title}}", "_" + title);
                }
                else
                {
                    str = str.Replace("{{title}}", title);
                }
                data = System.Text.Encoding.UTF8.GetBytes(str);
            }

            if (data != null)
            {
                retPath = FilePath + @"\" + fileName + _constantConfigService.XamlFileExtension;
                FileStream fileStream = new FileStream(retPath, FileMode.CreateNew);
                fileStream.Write(data, 0, (int)(data.Length));
                fileStream.Close();
            }

            return retPath;

        }





    }
}