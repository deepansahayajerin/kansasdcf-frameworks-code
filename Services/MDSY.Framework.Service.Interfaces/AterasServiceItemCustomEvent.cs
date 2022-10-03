using System.Runtime.Serialization;

namespace MDSY.Framework.Service.Interfaces
{
    /// <summary>
    /// Represents custom Events for Ateras Service Items
    /// </summary>
    [DataContract]
    public class AterasServiceItemCustomEvent : IAterasServiceItem
    {
        #region public properties
        /// <summary>
        /// Name of custom event
        /// </summary>
        [DataMember]
        public string Name { get; private set; }

        /// <summary>
        /// Custom event ID
        /// </summary>
        [DataMember]
        public string Id { get; set; }

        /// <summary>
        /// Event's value
        /// </summary>
        [DataMember]
        public string Value { get; set; }

        #endregion

        #region constructors
        /// <summary>
        /// Initializes a new instance of the AterasServiceItemCustomEvent class.
        /// </summary>
        /// <param name="id">Event ID</param>
        /// <param name="value">Specifies the event</param>
        public AterasServiceItemCustomEvent(string id, string value)
        {
            Id = id;
            Value = value;
        }

        #endregion

        public delegate void CustomEventHandler(string id, string value);

    }

}