using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.IO;
using System.Linq;
using FlowSimulation.Contracts.Services;
using FlowSimulation.Contracts.Services.Metadata;
using FlowSimulation.Helpers.DesignPatterns;

namespace FlowSimulation.Managers
{
    class ServiceManager : Singleton<ServiceManager>
    {
        #region Private Class Members

        static string ModulePath = System.Environment.CurrentDirectory;

        /// <summary>
        /// Коллекция метаданных всех модулей
        /// </summary>
        IEnumerable<IServiceManagerMetadata> _serviceManagersMetadata;

        /// <summary>
        /// Список подключенных визуальных модулей
        /// </summary>
        [ImportMany(typeof(IServiceManager), AllowRecomposition = true)]
        private IEnumerable<Lazy<IServiceManager, IServiceManagerMetadata>> _externalServiceManagers;

        // Контейнер композиции
        private CompositionContainer _container;
        #endregion

        #region Ctor

        /// <summary>
        /// Конструктор
        /// </summary>
        private ServiceManager()
        {
            Compose();
        }
        #endregion

        #region Public Properties
        /// <summary>
        /// Метаданные всех модулей
        /// </summary>
        public IEnumerable<IServiceManagerMetadata> ServiceManagersMetadata
        {
            get
            {
                if (_serviceManagersMetadata == null)
                {
                    _serviceManagersMetadata = from am in _externalServiceManagers select am.Metadata;
                }
                return _serviceManagersMetadata;
            }
        }

        public IEnumerable<Lazy<IServiceManager, IServiceManagerMetadata>> ServiceManagers
        {
            get
            {
                return _externalServiceManagers;
            }
        }

        #endregion

        #region Public Methods

        public IServiceManager GetManagerByInnerCode(string code)
        {
            var managerMetadata = _externalServiceManagers.FirstOrDefault(i => i.Metadata.Code == code);
            return managerMetadata == null ? null : managerMetadata.Value;
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
                Console.WriteLine("Всего менеджеров агентов: " + ServiceManagersMetadata.Count());
            }
        }
        #endregion
    }
}
