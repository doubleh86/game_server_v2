using System.ComponentModel;
using System.Data;

namespace AdminWeb.Services;

public static class DataTableExtensions
{
    public static DataTable ToDataTable<T>(this IList<T> data)
    {
        PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(typeof(T));
        DataTable table = new DataTable();
        foreach (PropertyDescriptor prop in properties)
            table.Columns.Add(prop.Name, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType);
        foreach (T item in data)
        {
            DataRow row = table.NewRow();
            foreach (PropertyDescriptor prop in properties)
            {
                var value = prop.GetValue(item) ?? DBNull.Value;
                if (value is string == true && ((string)value).Contains(",") == true)
                {
                    var listString = ((string)value).Split(",");
                    for (int i = 0; i < listString.Length; i++)
                    {
                        string propName = prop.Name;
                        if(i > 0)
                            propName = $"{prop.Name}_{i}";

                        if (table.Columns.Contains(propName) == false)
                        {
                            table.Columns.Add(propName, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType);
                        }

                        row[propName] = listString[i];
                    }

                    continue;

                }

                row[prop.Name] = value;
            }

            table.Rows.Add(row);
        }
        return table;
    }
}
