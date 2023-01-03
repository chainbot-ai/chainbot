using Chainbot.Contracts.Activities;
using Chainbot.Contracts.AppDomains;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Diagnostics;
using Plugins.Shared.Library;
using System.IO;

namespace Chainbot.Cores.AppDomains
{
    public class AssemblyResolveService: IAssemblyResolveService
    {
        private List<string> _assemblies = new List<string>();

        private List<string> GetBinAssemblies()
        {
            List<string> list = new List<string>();
            try
            {
                foreach (string text in Directory.EnumerateFiles(AppDomain.CurrentDomain.BaseDirectory, "*.dll"))
                {
                    try
                    {
                        list.Add(text);
                    }
                    catch (Exception ex)
                    {
                        Trace.TraceError(ex.ToString());
                    }
                }
            }
            catch (Exception ex2)
            {
                Trace.TraceError(ex2.ToString());
            }
            return list;
        }

        private void LoadBinReferencedAssemblies()
        {
            var list = GetBinAssemblies();
            foreach (var assemblyFile in list)
            {
                try
                {
                    var fileName = Path.GetFileNameWithoutExtension(assemblyFile);
                    if (IgnoreAssemblyName(fileName))
                    {
                        continue;
                    }

                    var asms = Assembly.LoadFrom(assemblyFile).GetReferencedAssemblies();
                    foreach (var item in asms)
                    {
                        try
                        {
                            if (IgnoreAssemblyName(item.Name))
                            {
                                continue;
                            }

                            Assembly.Load(item);
                        }
                        catch (Exception)
                        {

                        }
                    }
                }
                catch (Exception)
                {

                }

            }
        }

        private bool IgnoreAssemblyName(string name)
        {
            var names = new string[] { "NPinyinPro" };

            if (names.Contains(name))
            {
                return true;
            }

            return false;
        }

        public AssemblyResolveService()
        {
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;

            LoadBinReferencedAssemblies();
        }

        public void Init(List<string> assemblies)
        {
            _assemblies = assemblies;
        }

        private Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            var name = args.Name.Split(',')[0];

            var path = _assemblies.Where(item => System.IO.Path.GetFileNameWithoutExtension(item).Equals(name)).FirstOrDefault();

            if (System.IO.File.Exists(path))
            {
                try
                {
                    var assembly = Assembly.LoadFrom(path);
                    return assembly;
                }
                catch (Exception)
                {
                    return null;
                }
                
            }
            else
            {
                path = Path.Combine(SharedObject.Instance.ApplicationCurrentDirectory, name + ".dll");
                if (System.IO.File.Exists(path))
                {
                    try
                    {
                        var assembly = Assembly.LoadFrom(path);
                        return assembly;
                    }
                    catch (Exception)
                    {
                        return null;
                    }
                    
                }

                Trace.WriteLine(string.Format("********************{0} assembly could not be found********************", args.Name));
            }

            return null;
        }


    }
}
