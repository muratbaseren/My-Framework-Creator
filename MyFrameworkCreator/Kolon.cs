using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyFrameworkCreator
{
    public class Kolon
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string TypeName { get; set; }
        public bool IsNullable { get; set; }
        public int? MaxLength { get; set; }
        public bool IsIdentity { get; set; }

        public string GetCSharpPropText()
        {
            string typ = "string";

            switch (TypeName)
            {
                case "char":
                case "varchar":
                case "nvarchar":
                case "text":
                    typ = "string";
                    break;
                case "int":
                case "smallint":
                case "bigint":
                    typ = "int";
                    break;
                case "decimal":
                case "float":
                    typ = "decimal";
                    break;
                case "bit":
                    typ = "bool";
                    break;
                case "date":
                case "datetime":
                case "smaldatetime":
                    typ = "DateTime";
                    break;
                case "tinyint":
                    typ = "byte";
                    break;
                case "uniqueidentifier":
                    typ = "Guid";
                    break;
                case "image":
                    typ = "byte[]";
                    break;
                default:
                    typ = "object";
                    break;
            }

            return "public " + typ + " " + Name + " { get; set; }";
        }

        public override string ToString()
        {
            // Kolon adı gösterilir.
            return Name;
        }
    }
}
