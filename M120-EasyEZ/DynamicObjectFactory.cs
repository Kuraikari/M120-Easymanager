using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace M120_EasyEZ
{
    public class DynamicObjectFactory
    {
        public int Id { get; set; }
        public string Name { get; set; }
        private List<dynamic> Dynamics { get; set; }

        public DynamicObjectFactory()
        {
            Dynamics = new List<dynamic>();
        }

        public void Add(dynamic o)
        {
            Dynamics.Add(o);
        }

        public void Remove(dynamic o)
        {
            Dynamics.Remove(o);
        }

        public dynamic New()
        {
            dynamic o = null;
            Dynamics.Add(o);
            return o;
        }

        public dynamic New(bool isExpando, bool hasDynamicProps = false, string[] props = null)
        {
            dynamic o = null;
            props = props ?? new string[0];

            if (isExpando)
            {
                o = ExpandDynamicObject(o);
            }
            if (hasDynamicProps)
            {
                o = AddPropertiesToObject(o, props);
            }
            Dynamics.Add(o);
            return o;
        }

        private ExpandoObject ExpandDynamicObject(dynamic o)
        {
            if (o == null) {
                o = new ExpandoObject() as IDictionary<string, Object>;
                return o;
            }
            else
            {
                return null;
            }
        }

        private ExpandoObject AddPropertiesToObject(IDictionary<string, object> o, string[] props)
        {
            dynamic obj = new ExpandoObject() as IDictionary<string, object>;

            foreach (string prop in props) {
                //If prop is empty
                if (prop == "" || prop == " ") return obj;

                o.Add(prop, string.Empty);
            }

            obj = o;
            return obj;
        }

        public List<dynamic> GetDynamicList()
        {
            return this.Dynamics;
        }
    }
}
