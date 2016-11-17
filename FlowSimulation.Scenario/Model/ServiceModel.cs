using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace FlowSimulation.Scenario.Model
{
    [Serializable]
    public sealed class ServiceModel : ISerializable
    {
        
        public ServiceModel(ulong id, string name, Contracts.Services.Metadata.IServiceManagerMetadata serviceMetadata, Dictionary<string, object> settings)
        {
            Id = id;

            _name = name;
            _managerCode = serviceMetadata.Code;
            _typeName = serviceMetadata.FancyName;
            _settings = settings;
        }

        public ServiceModel(SerializationInfo info, StreamingContext context)
        {
            try
            {
                this.Id = info.GetUInt64("Id");
                this.Name = info.GetString("Name");
                this.TypeName = info.GetString("TypeName");
                this.Settings = (Dictionary<string, object>)info.GetValue("Settings", typeof(Dictionary<string, object>));
                this.ManagerCode = info.GetString("ManagerCode");
            }
            catch (SerializationException ex)
            {
                System.Diagnostics.Debug.WriteLine(string.Format("Тип: {0} Ошибка:{1} Сообщение:{2}", this.GetType().Name, ex.GetType().Name, ex.Message));
            }
        }

        public ulong Id { get; private set; }

        private Dictionary<string, object> _settings;
        public Dictionary<string, object> Settings
        {
            get { return _settings; }
            set { _settings = value; }
        }

        private string _name;
        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        private string _typeName;
        public string TypeName
        {
            get { return _typeName; }
            set { _typeName = value; }
        }

        private string _managerCode;
        public string ManagerCode
        {
            get { return _managerCode; }
            set { _managerCode = value; }
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Id", Id);
            info.AddValue("Name", Name);
            info.AddValue("TypeName", TypeName);
            info.AddValue("Settings", Settings);
            info.AddValue("ManagerCode", ManagerCode);
        }
    }
}
