using Chainbot.Contracts.Log;
using Chainbot.Contracts.UI;
using ChainbotStudio.Views;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using log4net;
using System;
using System.Windows;
using System.Windows.Controls;

namespace ChainbotStudio.ViewModel
{
    /// <summary>
    /// This class contains properties that a View can data bind to.
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class AccountPageViewModel : ViewModelBase
    {
        private static readonly ILog _logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private ILogService _logService;
        private IMessageBoxService _messageBoxService;

        private AccountPageView _view;

        /// <summary>
        /// Initializes a new instance of the AccountPageViewModel class.
        /// </summary>
        public AccountPageViewModel(IMessageBoxService messageBoxService, ILogService logService)
        {
            _messageBoxService = messageBoxService;
            _logService = logService;
        }

        /// <summary>
        /// The <see cref="UserName" /> property's name.
        /// </summary>
        public const string UserNamePropertyName = "UserName";

        private string _userNameProperty = "";

        /// <summary>
        /// Sets and gets the UserName property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string UserName
        {
            get
            {
                return _userNameProperty;
            }

            set
            {
                if (_userNameProperty == value)
                {
                    return;
                }

                _userNameProperty = value;
                RaisePropertyChanged(UserNamePropertyName);


                IsUserNameWrong = false;
                ErrMsg = "";
            }
        }

        /// <summary>
        /// The <see cref="Password" /> property's name.
        /// </summary>
        public const string PasswordPropertyName = "Password";

        private string _passwordProperty = "";

        /// <summary>
        /// Sets and gets the Password property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string Password
        {
            get
            {
                return _passwordProperty;
            }

            set
            {
                if (_passwordProperty == value)
                {
                    return;
                }

                _passwordProperty = value;
                RaisePropertyChanged(PasswordPropertyName);

                IsPassWordWrong = false;
                ErrMsg = "";
            }
        }

        /// <summary>
        /// The <see cref="Key" /> property's name.
        /// </summary>
        public const string KeyPropertyName = "Key";

        private string _keyProperty = "";

        /// <summary>
        /// Sets and gets the Key property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string Key
        {
            get
            {
                return _keyProperty;
            }

            set
            {
                if (_keyProperty == value)
                {
                    return;
                }

                _keyProperty = value;
                RaisePropertyChanged(KeyPropertyName);

                IsKeyWrong = false;
                ErrMsg = "";
            }
        }

        /// <summary>
        /// The <see cref="Rememb" /> property's name.
        /// </summary>
        public const string RemembPropertyName = "Rememb";

        private bool _remembProperty = false;

        /// <summary>
        /// Sets and gets the Rememb property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool Rememb
        {
            get
            {
                return _remembProperty;
            }

            set
            {
                if (_remembProperty == value)
                {
                    return;
                }

                _remembProperty = value;
                RaisePropertyChanged(RemembPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="AutoLogin" /> property's name.
        /// </summary>
        public const string AutoLoginPropertyName = "AutoLogin";

        private bool _autoLoginProperty = false;

        /// <summary>
        /// Sets and gets the AutoLogin property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool AutoLogin
        {
            get
            {
                return _autoLoginProperty;
            }

            set
            {
                if (_autoLoginProperty == value)
                {
                    return;
                }

                _autoLoginProperty = value;
                RaisePropertyChanged(AutoLoginPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="ErrMsg" /> property's name.
        /// </summary>
        public const string ErrMsgPropertyName = "ErrMsg";

        private string _errMsgProperty = "";

        /// <summary>
        /// Sets and gets the ErrMsg property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string ErrMsg
        {
            get
            {
                return _errMsgProperty;
            }

            set
            {
                if (_errMsgProperty == value)
                {
                    return;
                }

                _errMsgProperty = value;
                RaisePropertyChanged(ErrMsgPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="IsUserNameWrong" /> property's name.
        /// </summary>
        public const string IsUserNameWrongPropertyName = "IsUserNameWrong";

        private bool _isUserNameWrongProperty = false;

        /// <summary>
        /// Sets and gets the IsUserNameWrong property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool IsUserNameWrong
        {
            get
            {
                return _isUserNameWrongProperty;
            }

            set
            {
                if (_isUserNameWrongProperty == value)
                {
                    return;
                }

                _isUserNameWrongProperty = value;
                RaisePropertyChanged(IsUserNameWrongPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="IsPassWordWrong" /> property's name.
        /// </summary>
        public const string IsPassWordWrongPropertyName = "IsPassWordWrong";

        private bool _isPassWordWrongProperty = false;

        /// <summary>
        /// Sets and gets the IsPassWordWrong property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool IsPassWordWrong
        {
            get
            {
                return _isPassWordWrongProperty;
            }

            set
            {
                if (_isPassWordWrongProperty == value)
                {
                    return;
                }

                _isPassWordWrongProperty = value;
                RaisePropertyChanged(IsPassWordWrongPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="IsKeyWrong" /> property's name.
        /// </summary>
        public const string IsKeyWrongPropertyName = "IsKeyWrong";

        private bool _isKeyWrongProperty = false;

        /// <summary>
        /// Sets and gets the IsKeyWrong property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool IsKeyWrong
        {
            get
            {
                return _isKeyWrongProperty;
            }

            set
            {
                if (_isKeyWrongProperty == value)
                {
                    return;
                }

                _isKeyWrongProperty = value;
                RaisePropertyChanged(IsKeyWrongPropertyName);
            }
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
                        _view = (AccountPageView)p.Source;
                        _view.UserName.Focus();
                    }));
            }
        }

        private RelayCommand _loginCommand;

        /// <summary>
        /// Gets the LoginCommand.
        /// </summary>
        public RelayCommand LoginCommand
        {
            get
            {
                return _loginCommand
                    ?? (_loginCommand = new RelayCommand(
                    () =>
                    {
                        DealLoginInfo();
                    }));
            }
        }

        private void DealLoginInfo()
        {
            try
            {
                IsUserNameWrong = false;
                IsPassWordWrong = false;
                IsKeyWrong = false;
                ErrMsg = "";
                if (string.IsNullOrEmpty(UserName))
                {
                    ErrMsg = "用户名不能为空！";
                    //IsUserNameWrong = true;
                    _view.UserName.Focus();
                    return;
                }
                else if (string.IsNullOrEmpty(Password))
                {
                    ErrMsg = "密码不能为空！";
                    //IsPassWordWrong = true;
                    _view.Password.Focus();
                    return;
                }
                else if (string.IsNullOrEmpty(Key))
                {
                    ErrMsg = "验证码不能为空！";
                    //IsKeyWrong = true;
                    _view.Key.Focus();
                    return;
                }

                AsyncLogin();
            }
            catch (Exception err)
            {
                ErrMsg = "登录过程中出现了异常，请联系软件开发商！";
                _logService.Error(err, _logger);
            }
        }

        private void AsyncLogin()
        {
            _messageBoxService.ShowInformation("登录成功！");
        }
    }
}