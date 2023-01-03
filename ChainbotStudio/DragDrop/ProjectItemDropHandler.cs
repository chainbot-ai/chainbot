using ChainbotStudio.ViewModel;
using GongSolutions.Wpf.DragDrop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using ReflectionMagic;
using Chainbot.Contracts.Log;
using log4net;
using Chainbot.Contracts.UI;
using Chainbot.Contracts.Utils;
using Chainbot.Cores.Classes;
using Chainbot.Contracts.App;

namespace ChainbotStudio.DragDrop
{
    public class ProjectItemDropHandler : DefaultDropHandler
    {
        private static readonly ILog _logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private IServiceLocator _serviceLocator;

        private IDispatcherService _dispatcherService;
        private ILogService _logService;
        private IMessageBoxService _messageBoxService;
        private ICommonService _commonService;
        private DocksViewModel _docksViewModel;
        private ProjectViewModel _projectViewModel;

        public ProjectItemDropHandler(IServiceLocator serviceLocator)
        {
            _serviceLocator = serviceLocator;
            _dispatcherService = _serviceLocator.ResolveType<IDispatcherService>();

            _dispatcherService.InvokeAsync(()=> {
                _logService = _serviceLocator.ResolveType<ILogService>();
                _messageBoxService = _serviceLocator.ResolveType<IMessageBoxService>();
                _commonService = _serviceLocator.ResolveType<ICommonService>();
                _docksViewModel = _serviceLocator.ResolveType<DocksViewModel>();
                _projectViewModel = _serviceLocator.ResolveType<ProjectViewModel>();
            });
        }

        public override void DragOver(IDropInfo dropInfo)
        {
            var sourceItem = dropInfo.DragInfo.Data;
            var targetItem = dropInfo.TargetItem;

            dynamic drop = dropInfo.AsDynamic();
            drop.Data = sourceItem;

            bool bCanDrag = true;

            if (sourceItem is ProjectDirItemViewModel || sourceItem is ProjectFileItemViewModel)
            {
                if(targetItem is ProjectDirItemViewModel)
                {
                    var target = targetItem as ProjectDirItemViewModel;

                    do
                    {
                        if (!(System.IO.Directory.Exists(target.Path) && !target.IsScreenshots))
                        {
                            bCanDrag = false;
                            break;
                        }

                        if (sourceItem is ProjectDirItemViewModel)
                        {
                            var sourceDir = sourceItem as ProjectDirItemViewModel;
                            if (!(System.IO.Directory.Exists(sourceDir.Path) && !sourceDir.IsScreenshots))
                            {
                                bCanDrag = false;
                                break;
                            }

                            if ((target.Path + @"\" + sourceDir.Name).ToLower() == sourceDir.Path.ToLower())
                            {
                                bCanDrag = false;
                                break;
                            }
                        }

                        if (sourceItem is ProjectFileItemViewModel)
                        {
                            var sourceFile = sourceItem as ProjectFileItemViewModel;
                            if(sourceFile.IsProjectJsonFile)
                            {
                                bCanDrag = false;
                                break;
                            }

                            if ((target.Path + @"\" + sourceFile.Name).ToLower() == sourceFile.Path.ToLower())
                            {
                                bCanDrag = false;
                                break;
                            }

                        }


                    } while (false);
                }
                else if (targetItem is ProjectRootItemViewModel)
                {
                    var target = targetItem as ProjectRootItemViewModel;

                    do
                    {
                        if (sourceItem is ProjectDirItemViewModel)
                        {
                            var sourceDir = sourceItem as ProjectDirItemViewModel;
                            if (!(System.IO.Directory.Exists(sourceDir.Path) && !sourceDir.IsScreenshots))
                            {
                                bCanDrag = false;
                                break;
                            }

                            if ((target.Path + @"\" + sourceDir.Name).ToLower() == sourceDir.Path.ToLower())
                            {
                                bCanDrag = false;
                                break;
                            }
                        }

                        if (sourceItem is ProjectFileItemViewModel)
                        {
                            var sourceFile = sourceItem as ProjectFileItemViewModel;
                            if (sourceFile.IsProjectJsonFile)
                            {
                                bCanDrag = false;
                                break;
                            }

                            if ((target.Path + @"\" + sourceFile.Name).ToLower() == sourceFile.Path.ToLower())
                            {
                                bCanDrag = false;
                                break;
                            }
                        }

                    } while (false);

                }
                else
                {
                    bCanDrag = false;
                }

                if (bCanDrag)
                {
                    base.DragOver(dropInfo);

                    if (dropInfo.DropTargetAdorner == DropTargetAdorners.Insert)
                    {
                        dropInfo.Effects = DragDropEffects.None;
                        dropInfo.DropTargetAdorner = null;
                    }
                }

            }
            else
            {
                dropInfo.Effects = DragDropEffects.None;
            }
        }

        public override void Drop(IDropInfo dropInfo)
        {
            if (dropInfo.VisualTarget == dropInfo.DragInfo.VisualSource)
            {
                var sourceItem = dropInfo.Data as ProjectBaseItemViewModel;
                var targetItem = dropInfo.TargetItem as ProjectBaseItemViewModel;

                _logService.Debug(string.Format("sourceItem.Path={0}", sourceItem?.Path), _logger);
                _logService.Debug(string.Format("targetItem.Path={0}", targetItem?.Path), _logger);

                if (sourceItem != null && targetItem != null)
                {
                    if (System.IO.File.Exists(sourceItem.Path))
                    {
                        if (MoveFileToDir(sourceItem as ProjectFileItemViewModel, targetItem))
                        {
                            base.Drop(dropInfo);
                            _projectViewModel.Refresh();
                        }

                        
                    }
                    else
                    {
                        if (MoveDirToDir(sourceItem, targetItem))
                        {
                            base.Drop(dropInfo);
                            _projectViewModel.Refresh();
                        }

                       
                    }

                }


            }
        }




        private bool MoveDirToDir(ProjectBaseItemViewModel sourceItem, ProjectBaseItemViewModel targetItem)
        {
            var srcPath = sourceItem.Path;
            var dstPath = targetItem.Path;

            var dstPathCombine = System.IO.Path.Combine(dstPath, sourceItem.Name);

            if (System.IO.Directory.Exists(dstPathCombine))
            {
                _messageBoxService.ShowWarning(Chainbot.Resources.Properties.Resources.Message_DropError1);
            }
            else
            {
                System.IO.Directory.Move(srcPath, dstPathCombine);


                foreach (var file in System.IO.Directory.GetFiles(dstPathCombine, "*.*"))
                {
                    var relativeFile = _commonService.MakeRelativePath(dstPathCombine, file);
                    var srcFile = System.IO.Path.Combine(srcPath, relativeFile);

                    foreach (var doc in _docksViewModel.Documents)
                    {
                        if (doc.Path.EqualsIgnoreCase(srcFile))
                        {
                            doc.Path = file;
                            doc.ToolTip = doc.Path;
                            doc.UpdatePathCrossDomain(doc.Path);
                            break;
                        }
                    }
                }

                return true;
            }



            return false;
        }

        private bool MoveFileToDir(ProjectFileItemViewModel sourceItem, ProjectBaseItemViewModel targetItem)
        {
            var srcFile = sourceItem.Path;
            var dstPath = targetItem.Path;

            var dstFile = System.IO.Path.Combine(dstPath, sourceItem.Name);
            if (System.IO.File.Exists(dstFile))
            {
                _messageBoxService.ShowWarning(Chainbot.Resources.Properties.Resources.Message_DropError2);
            }
            else
            {
                System.IO.File.Move(srcFile, dstFile);
                sourceItem.Path = dstFile;
                foreach (var doc in _docksViewModel.Documents)
                {
                    if (doc.Path.EqualsIgnoreCase(srcFile))
                    {
                        doc.Path = dstFile;
                        doc.ToolTip = doc.Path;
                        doc.UpdatePathCrossDomain(doc.Path);
                        break;
                    }
                }

                if (sourceItem.IsMainXamlFile)
                {
                    sourceItem.SetAsMainCommand.Execute(null);
                }

                return true;
            }

            return false;
        }








    }
}
