using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.IO;
using System.Linq;
using FlowSimulation.Contracts.ViewPort.Metadata;
using FlowSimulation.Contracts.ViewPort;
using FlowSimulation.Helpers.DesignPatterns;

namespace FlowSimulation.Managers
{
    class ViewPortManager : Singleton<ViewPortManager>
    {
        #region Private Class Members

        static string ModulePath = System.Environment.CurrentDirectory;

        /// <summary>
        /// Коллекция метаданных всех модулей
        /// </summary>
        IEnumerable<IViewPortMetadata> _viewPortMetadata;

        /// <summary>
        /// Список подключенных визуальных модулей
        /// </summary>
        [ImportMany(typeof(IViewPort), AllowRecomposition = true)]
        private IEnumerable<Lazy<IViewPort, IViewPortMetadata>> _viewPortContainer;

        // Контейнер композиции
        private CompositionContainer _container;
        #endregion

        #region Ctor

        /// <summary>
        /// Конструктор
        /// </summary>
        private ViewPortManager()
        {
            Compose();
        }
        #endregion

        #region Public Properties
        /// <summary>
        /// Метаданные всех модулей
        /// </summary>
        public IEnumerable<IViewPortMetadata> ViewPortMetadatas
        {
            get
            {
                if (_viewPortMetadata == null)
                {
                    _viewPortMetadata = from am in _viewPortContainer select am.Metadata;
                }
                return _viewPortMetadata;
            }
        }

        public IEnumerable<Lazy<IViewPort, IViewPortMetadata>> ViewPorts
        {
            get
            {
                return _viewPortContainer;
            }
        }

        #endregion

        #region Public Methods

        public IViewPort GetViewPortByCode(string code)
        {
            return _viewPortContainer.FirstOrDefault(i => i.Metadata.Code == code).Value;
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
                Console.WriteLine("Ошибка сборки: ", ex.Message);
            }
            finally
            {
                Console.WriteLine("Всего менеджеров агентов: ", ViewPortMetadatas.Count());
            }
        }
        #endregion
    }
}
