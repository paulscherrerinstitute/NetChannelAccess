using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using CaSharpServer.Constants;
using System.Globalization;
using System.Threading;

namespace CaSharpServer
{
    public class PropertyDelegateEventArgs : EventArgs
    {
        public string Property { get; set; }
        public object OldValue { get; set; }
        public object NewValue { get; set; }
        public bool CancelEvent;
    }

    /// <summary>
    /// Base class for all the server records.
    /// Offers the base fields as well as some of the logic.
    /// </summary>
    public abstract class CARecord
    {
        /// <summary>
        /// Called when the record is processed (for example to fire the EPICS monitor event).
        /// </summary>
        public event EventHandler RecordProcessed;
        /// <summary>
        /// Will be called BEFORE the record is processed (called before the EPICS monitor is called).
        /// </summary>
        public event EventHandler PrepareRecord;

        public event EventHandler<PropertyDelegateEventArgs> PropertySet;

        /// <summary>
        /// Defines the Scan algorithm used for this record
        /// </summary>
        [CAField("SCAN")]
        public ScanAlgorithm Scan { get; set; }

        /// <summary>
        /// Internal variable for the DisableAlarmServerity property
        /// </summary>
        private bool disableAlarmSeverity;

        /// <summary>
        /// Defines if the record will handle or not the alarm
        /// </summary>
        [CAField("DISS")]
        public bool DisableAlarmServerity
        {
            get
            {
                return disableAlarmSeverity;
            }
            set
            {
                if (disableAlarmSeverity != value)
                {
                    disableAlarmSeverity = value;
                    IsDirty = true;
                }
            }
        }

        /// <summary>
        /// Internal variable for the AlarmStatus property
        /// </summary>
        private AlarmStatus alarmStatus;

        /// <summary>
        /// Defines the current record alarm status
        /// </summary>
        [CAField("STAT")]
        public AlarmStatus AlarmStatus
        {
            get
            {
                return alarmStatus;
            }
            set
            {
                if (value != alarmStatus)
                {
                    alarmStatus = value;
                    IsDirty = true;
                }
            }
        }

        /// <summary>
        /// Internal variable for the AlarmSeverity property
        /// </summary>
        private AlarmSeverity currentAlarmSeverity;

        /// <summary>
        /// Defines the current alarm severity
        /// </summary>
        [CAField("SEVR")]
        public AlarmSeverity CurrentAlarmSeverity
        {
            get
            {
                return currentAlarmSeverity;
            }
            set
            {
                if (currentAlarmSeverity != value)
                {
                    currentAlarmSeverity = value;
                    IsDirty = true;
                }
            }
        }

        /// <summary>
        /// Internal variable for the AlarmAcknoledgeSeverity property
        /// </summary>
        private AlarmSeverity alarmAcknoledgeSeverity;

        /// <summary>
        /// Defines the highest unacknoledged alarm
        /// </summary>
        [CAField("ACKS")]
        public AlarmSeverity AlarmAcknoledgeSeverity
        {
            get
            {
                return alarmAcknoledgeSeverity;
            }
            set
            {
                if (alarmAcknoledgeSeverity != value)
                {
                    alarmAcknoledgeSeverity = value;
                    IsDirty = true;
                }
            }
        }

        /// <summary>
        /// Defines the name of the record (max 28 char.)
        /// </summary>
        [CAField("NAME")]
        public string Name { get; set; }

        /// <summary>
        /// Defines a description for this field (max 28 char.)
        /// </summary>
        [CAField("DESC")]
        public string Description { get; set; }

        protected DateTime lastProccessed = DateTime.Now;
        /// <summary>
        /// The time when this record was last processed.
        /// </summary>
        [CAField("TIME")]
        public DateTime LastProccessed { get { return lastProccessed; } internal set { lastProccessed = value; } }

        public bool CanBeRemotlySet { get; set; }

        /// <summary>
        /// Initialize the record with a scan rate of 10Hz and no alarm.
        /// </summary>
        internal CARecord()
        {
            Scan = ScanAlgorithm.HZ10;
            CurrentAlarmSeverity = AlarmSeverity.NO_ALARM;
            AlarmStatus = AlarmStatus.NO_ALARM;
            IsDirty = false;
            CanBeRemotlySet = true;
        }

        internal virtual void ProcessRecord()
        {
            lastProccessed = DateTime.Now;
            if (RecordProcessed != null)
                RecordProcessed(this, null);
            IsDirty = false;
        }

        /// <summary>
        /// Fires a change of the alarm status.
        /// </summary>
        /// <param name="severity">New alarm severity</param>
        /// <param name="status">New alarm status</param>
        public void TriggerAlarm(AlarmSeverity severity, AlarmStatus status)
        {
            if (CurrentAlarmSeverity != severity)
            {
                CurrentAlarmSeverity = severity;
                AlarmStatus = status;
            }
        }

        internal int dataCount = 1;

        object lockProps = new object();
        Dictionary<string, PropertyInfo> knownProps = null;
        private void PopulateProperties()
        {
            if (knownProps != null)
                return;

            knownProps = new Dictionary<string, PropertyInfo>();
            PropertyInfo[] props = this.GetType().GetProperties();
            foreach (var i in props)
            {
                CAFieldAttribute attr = (CAFieldAttribute)i.GetCustomAttributes(typeof(CAFieldAttribute), true).FirstOrDefault();
                if (attr != null)
                    knownProps.Add(attr.Name.ToUpper(), i);
            }
        }

        public object this[string key]
        {
            get
            {
                lock (lockProps)
                {
                    PopulateProperties();
                    if (!knownProps.ContainsKey(key.ToUpper()))
                        return null;
                    return knownProps[key.ToUpper()].GetValue(this, null);
                }
            }
            set
            {
                lock (lockProps)
                {
                    PopulateProperties();
                    if (!knownProps.ContainsKey(key.ToUpper()))
                        return;

                    if (PropertySet != null)
                    {
                        PropertyDelegateEventArgs arg = new PropertyDelegateEventArgs { Property = key.ToUpper(), OldValue = this[key.ToUpper()], NewValue = value, CancelEvent = false };
                        PropertySet(this, arg);
                        if (arg.CancelEvent == true)
                        {
                            IsDirty = true;
                            return;
                        }
                    }

                    if (knownProps[key.ToUpper()].PropertyType.IsArray)
                    {
                        int nb = Math.Min(((Array)knownProps[key.ToUpper()].GetValue(this, null)).Length, ((Array)value).Length);
                        for (int i = 0; i < nb; i++)
                            SetArrayValue(key, i, value);
                        /*for (int i = 0; i < nb; i++)
                            knownProps[key.ToUpper()].SetValue(this, value, new object[] { i });*/
                    }
                    else
                    {
                        knownProps[key.ToUpper()].SetValue(this, value, null);
                    }
                }
            }
        }

        /// <summary>
        /// Calls the PropertySet call back.
        /// If cancelled then return false, otherwise return true.
        /// </summary>
        /// <param name="evt"></param>
        /// <returns></returns>
        internal bool CallPropertySet(PropertyDelegateEventArgs evt)
        {
            if (PropertySet != null)
            {
                PropertySet(this, evt);
                if (evt.CancelEvent == true)
                {
                    IsDirty = true;
                    return false;
                }
            }
            return true;
        }

        public void SetArrayValue(string key, int index, object value)
        {
            lock (lockProps)
            {
                PopulateProperties();
                if (!knownProps.ContainsKey(key.ToUpper()))
                    return;
                Type t = knownProps[key.ToUpper()].PropertyType;
                if (t.Name.Split('`')[0] == "ArrayContainer")
                    t = t.GetGenericArguments().First();
                else
                    t = t.GetElementType();
                dynamic arr = (dynamic)knownProps[key.ToUpper()].GetValue(this, null);
                switch (t.Name)
                {
                    case "Byte":
                        {
                            arr[index] = (byte)value;
                            break;
                        }
                    case "Int32":
                        {
                            arr[index] = (int)value;
                            break;
                        }
                    case "Double":
                        {
                            arr[index] = (double)value;
                            break;
                        }
                    case "Single":
                        {
                            arr[index] = (float)value;
                            break;
                        }
                    case "Short":
                        {
                            arr[index] = (short)value;
                            break;
                        }
                    case "String":
                        {
                            arr[index] = (string)value;
                            break;
                        }
                    default:
                        throw new Exception("Array type not supported.");
                }
                //knownProps[key.ToUpper()].SetValue(this, value, new object[] { index });
            }
        }

        public Type GetPropertyType(string key)
        {
            lock (lockProps)
            {
                PopulateProperties();
                if (!knownProps.ContainsKey(key.ToUpper()))
                    return null;
                return knownProps[key.ToUpper()].PropertyType;
            }
        }

        internal void CallPrepareRecord()
        {
            if (PrepareRecord != null)
                PrepareRecord(this, null);
        }

        public int GetInt(string key)
        {
            object val = this[key];
            if (val == null)
                return 0;
            if (val is double)
                return (int)((double)val);
            else if (val is int)
                return (int)val;
            else if (val is float)
                return (int)((float)val);
            else if (val is short)
                return (int)((short)val);
            else if (val is long)
                return (int)((long)val);
            else if (val is string)
                return int.Parse((string)val, CultureInfo.InvariantCulture);
            throw new Exception("Doesn't support convertion from type " + val.GetType().FullName);
        }

        public double GetDouble(string key)
        {
            object val = this[key];
            if (val == null)
                return 0;
            if (val is double)
                return (double)((double)val);
            else if (val is int)
                return (double)((int)val);
            else if (val is float)
                return (double)((float)val);
            else if (val is short)
                return (double)((short)val);
            else if (val is long)
                return (double)((long)val);
            else if (val is string)
                return double.Parse((string)val, CultureInfo.InvariantCulture);
            throw new Exception("Doesn't support convertion from type " + val.GetType().FullName);
        }

        public float GetFloat(string key)
        {
            object val = this[key];
            if (val == null)
                return 0;
            if (val is double)
                return (float)((double)val);
            else if (val is int)
                return (float)((int)val);
            else if (val is float)
                return (float)((float)val);
            else if (val is short)
                return (float)((short)val);
            else if (val is long)
                return (float)((long)val);
            else if (val is string)
                return float.Parse((string)val, CultureInfo.InvariantCulture);
            throw new Exception("Doesn't support convertion from type " + val.GetType().FullName);
        }

        public long GetLong(string key)
        {
            object val = this[key];
            if (val == null)
                return 0;
            if (val is double)
                return (long)((double)val);
            else if (val is int)
                return (long)((int)val);
            else if (val is float)
                return (long)((float)val);
            else if (val is short)
                return (long)((short)val);
            else if (val is long)
                return (long)((long)val);
            else if (val is string)
                return long.Parse((string)val, CultureInfo.InvariantCulture);
            throw new Exception("Doesn't support convertion from type " + val.GetType().FullName);
        }

        public short GetShort(string key)
        {
            object val = this[key];
            if (val == null)
                return 0;
            if (val is double)
                return (short)((double)val);
            else if (val is int)
                return (short)((int)val);
            else if (val is float)
                return (short)((float)val);
            else if (val is short)
                return (short)((short)val);
            else if (val is long)
                return (short)((long)val);
            else if (val is string)
                return short.Parse((string)val, CultureInfo.InvariantCulture);
            throw new Exception("Doesn't support convertion from type " + val.GetType().FullName);
        }

        public bool IsDirty { get; set; }

        /// <summary>
        /// The AtomicChange class is used for grouping multiple changes
        /// to a CARecord into one atomic change.
        /// </summary>
        private class AtomicChange : IDisposable
        {
            private CARecord record;
            bool withProcess = true;

            internal AtomicChange(CARecord record)
            {
                this.record = record;
                record.updateMutex.WaitOne();
            }

            internal AtomicChange(CARecord record, bool withProcess)
            {
                this.record = record;
                record.updateMutex.WaitOne();
                this.withProcess = withProcess;
            }

            public void Dispose()
            {
                record.updateMutex.ReleaseMutex();
                if (record.inAtomicDispose)
                    return;
                record.inAtomicDispose = true;
                if (record.IsDirty && record.Scan == ScanAlgorithm.ON_CHANGE && withProcess)
                    record.ProcessRecord();
                record.inAtomicDispose = false;
            }
        }

        /// <summary>
        /// Use this method with a using statement to group multiple changes into
        /// one logical unit of change. This allows for bulk updates, e.g. setting
        /// an entire array at once or changing alarm status, severity and value
        /// together as one cohesive change.
        /// </summary>
        /// <returns>This method returns a guarding object, that you can use
        /// to group multiple changes into one big atomic change.</returns>
        /// <example>
        /// <![CDATA[
        /// var record = CAServer.CreateArrayRecord<int>(8);
        /// using (record.CreateAtomicChange())
        /// {
        ///     for (var i = 0; i < record.Value.Length; i++)
        ///         record.Value[i] = i * 5;
        /// }
        /// ]]>
        /// </example>
        public IDisposable CreateAtomicChange()
        {
            return new AtomicChange(this);
        }

        internal IDisposable CreateAtomicChange(bool withProcess)
        {
            return new AtomicChange(this, withProcess);
        }

        /// <summary>
        /// Mutex used for synchronizing (atomic) changes.
        /// </summary>
        private Mutex updateMutex = new Mutex();

        /// <summary>
        /// Flag for preventing re-creating AtomicChange instances from
        /// AtomicChange.Dispose if the record's ScanAlgorithm is ON_CHANGE.
        /// </summary>
        bool inAtomicDispose = false;
    }
}
