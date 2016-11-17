using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.IO;
using System.Linq;
using FlowSimulation.Contracts.Agents;
using FlowSimulation.Contracts.Agents.Metadata;
using FlowSimulation.Helpers.DesignPatterns;

namespace FlowSimulation.Managers
{
    /// <summary>
    /// Менеджер модулей
    /// </summary>    
    class AgentManager : Singleton<AgentManager>, IDisposable
    {
        #region Private Class Members

        static string ModulePath = System.Environment.CurrentDirectory;

        /// <summary>
        /// Коллекция метаданных всех модулей
        /// </summary>
        IEnumerable<IAgentManagerMetadata> _agentManagersMetadata;

        /// <summary>
        /// Список подключенных визуальных модулей
        /// </summary>
        [ImportMany(typeof(IAgentManager), AllowRecomposition = true)]
        private IEnumerable<Lazy<IAgentManager, IAgentManagerMetadata>> _externalAgentManagers;

        // Контейнер композиции
        private CompositionContainer _container;
        private List<string> _codes;
        #endregion

        #region Ctor

        /// <summary>
        /// Конструктор
        /// </summary>
        private AgentManager()
        {
            Compose();
        }
        #endregion

        #region Public Properties
        /// <summary>
        /// Метаданные всех модулей
        /// </summary>
        public IEnumerable<IAgentManagerMetadata> AgentManagersMetadata
        {
            get
            {
                if (_agentManagersMetadata == null)
                {
                    _agentManagersMetadata = from am in _externalAgentManagers select am.Metadata;
                }
                return _agentManagersMetadata;
            }
        }

        public IEnumerable<Lazy<IAgentManager, IAgentManagerMetadata>> AgentManagers
        {
            get
            {
                return _externalAgentManagers;
            }
        }

        #endregion

        #region Public Methods

        public IAgentManager GetManagerByInnerCode(string code)
        {
            var mng = _externalAgentManagers.FirstOrDefault(i => i.Metadata.Code == code);
            return mng == null ? null : mng.Value;
        }

        /// <summary>
        /// Метод делает композицию модулей
        /// </summary>
        private void Compose()
        {
            AggregateCatalog catalog = new AggregateCatalog();

            if (!Directory.Exists(ModulePath))
            {
                Directory.CreateDirectory(ModulePath);
            }

            foreach (var assemplyPath in Directory.EnumerateFiles(ModulePath, "*.dll"))
            {
                var assCat = new AssemblyCatalog(System.Reflection.Assembly.LoadFrom(assemplyPath));
                try
                {
                    if (assCat.Parts.Count() != 0)
                    {
                        catalog.Catalogs.Add(assCat);
                    }
                }
                catch (System.Reflection.ReflectionTypeLoadException ex)
                {
                    Console.WriteLine("Ошибка композиции:");
                    foreach (var e in ex.LoaderExceptions)
                    {
                         Console.WriteLine(e.Message);
                    }
                    //foreach (var e in ex.LoaderExceptions)
                    //{
                    //    string message = string.Format("[{0}]: {1}", assCat.Assembly.FullName, e.Message);
                    //    if (!Properties.Settings.Default.CompositionException.Contains(message))
                    //    {
                    //        Properties.Settings.Default.CompositionException.Add(message);
                    //        System.Diagnostics.Debug.WriteLine(message);
                    //    }
                    //}
                }
            }

            //Create the CompositionContainer with the parts in the catalog
            _container = new CompositionContainer(catalog, true);

            //Fill the imports of this object
            try
            {
                this._container.SatisfyImportsOnce(this);
            }
            catch (CompositionException compositionException)
            {
                Console.WriteLine("Ошибка сборки: " + compositionException.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка сборки: " + ex.Message);
            }
            finally
            {
                Console.WriteLine("Всего менеджеров агентов: " + AgentManagersMetadata.Count());
            }
        }
        #endregion

        public void Dispose()
        {
            if (_container != null)
            {
                _container.Dispose();
            }
        }
    }
}
