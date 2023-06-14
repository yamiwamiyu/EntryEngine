using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using OfficeOpenXml;
using System.Reflection;

public class ExcelData
{
    /// <summary>Sheet名，不填则默认按索引读取Sheet</summary>
    public string SheetName;
    /// <summary>是否忽略表头，忽略表头代表全是数据</summary>
    public bool IgnoreTitle;
    /// <summary>读取数据的类型</summary>
    public Type T;
    /// <summary>表头，Key为Excel表头字段名，Value为T类型字段名
    /// <para>不填则默认表头字段名和T类型字段名一致</para>
    /// <para>Value为空则仅检测表头字段名</para>
    /// <para>Key的顺序和Excel的表头字段的顺序可以不一致</para>
    /// </summary>
    public Dictionary<string, string> Title;
    /// <summary>读取到表头的行之后，可能后面几行也还是其它表头，通过这个偏移来过掉多余的表头</summary>
    public int RowOffset;
    /// <summary>读取到的数据集</summary>
    public List<object> Results;
    /// <summary>获取数据集</summary>
    public List<T> GetResults<T>()
    {
        return this.Results.Select(i => (T)i).ToList();
    }
    public void SetResults<T>(List<T> list)
    {
        this.Results = new List<object>(list.Select(i => (object)i));
    }
}

/// <summary>EPPlus插件，只能读xlsx，读大量数据速度比NPOI快</summary>
public static class _EPPLUS
{
    public class ReadExcelSheet : ExcelData
    {
        /// <summary>读取一行数据，不填则默认按照字段名自动读取</summary>
        public Func<ExcelRangeRow, object> Row;
    }
    public class WriteExcelSheet : ExcelData
    {
        /// <summary>读取一行数据，不填则默认按照字段名自动读取</summary>
        public Action<object, ExcelRangeRow> Row;
    }

    static _EPPLUS()
    {
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
    }
    /// <summary>读取Excel文件</summary>
    /// <param name="excel">Excel文件路径</param>
    /// <param name="loaders">读取表的方法</param>
    public static void LoadFromExcel(string excel, params ReadExcelSheet[] loaders)
    {
        using (var package = new ExcelPackage(excel))
        {
            var workbook = package.Workbook;
            for (int i = 0; i < workbook.Worksheets.Count; i++)
            {
                var sheet = workbook.Worksheets[i];
                var loader = loaders.FirstOrDefault(item => item.SheetName == sheet.Name);
                if (loader == null)
                {
                    if (i >= loaders.Length)
                        continue;
                    loader = loaders[i];
                }

                if (loader.T == null)
                {
                    if (!loader.IgnoreTitle && loader.Title == null)
                        throw new ArgumentNullException("自动读取表头字段T类型不能为空");
                    if (loader.Row == null)
                        throw new ArgumentNullException("自动读取数据T类型不能为空");
                }

                int start = 1;
                // 数据列和字段名的映射
                Dictionary<int, FieldInfo> fieldMapper = new Dictionary<int, FieldInfo>();
                FieldInfo[] fields = loader.T == null ? null : loader.T.GetFields(BindingFlags.Instance | BindingFlags.Public);
                if (!loader.IgnoreTitle)
                {
                    Dictionary<string, string> title;
                    if (loader.Title == null)
                    {
                        // 根据T类型自动读取表头
                        title = new Dictionary<string, string>();
                        foreach (var item in fields)
                            title[item.Name] = item.Name;
                    }
                    else
                    {
                        title = loader.Title;
                    }
                    for (int l = sheet.Dimension.Rows; start <= l; start++)
                    {
                        var row = sheet.Rows[start];
                        fieldMapper.Clear();
                        var titles = Enumerable.Range(1, row.Range.Columns).Select(i => row.Cell(i).GetTitle()).ToList();
                        foreach (var item in title)
                        {
                            int index = titles.IndexOf(item.Key);
                            if (index != -1)
                                fieldMapper[index + 1] = item.Value == null ? (fields == null ? null : fields[fieldMapper.Count]) : loader.T.GetField(item.Value);
                            else
                                // 本行不是表头
                                break;
                        }
                        if (fieldMapper.Count > 0)
                        {

                        }
                        if (fieldMapper.Count == title.Count)
                        {
                            // 正确找到了表头
                            start++;
                            break;
                        }
                    }
                    if (fieldMapper.Count == 0)
                    {
                        throw new KeyNotFoundException("未能找到表头行：" + string.Join(",", loader.Title.Keys.ToArray()));
                    }
                }

                start += loader.RowOffset;
                loader.Results = new List<object>();
                // 忽略表头时可能全是数据，并且表字段和类型字段顺序必须一致
                if (loader.Row == null && fieldMapper.Count == 0)
                {
                    int column = 1;
                    foreach (var item in fields)
                        fieldMapper[column++] = item;
                }

                for (int j = start, l = sheet.Dimension.Rows; j <= l; j++)
                {
                    var row = sheet.Rows[j];
                    try
                    {
                        if (loader.Row == null)
                        {
                            object data = Activator.CreateInstance(loader.T);
                            foreach (var item in fieldMapper)
                                item.Value.SetValue(data, row.Cell(item.Key).GetValue(item.Value.FieldType));
                            loader.Results.Add(data);
                        }
                        else
                        {
                            loader.Results.Add(loader.Row(row));
                        }
                    }
                    catch (Exception ex)
                    {
                        throw new Exception(string.Format("第{0}行数据导入错误，错误原因：{1}", row.StartRow, ex.Message));
                    }
                }
            }
        }
    }
    public static List<T> LoadFromExcel<T>(string excel, ReadExcelSheet loader)
    {
        loader.T = typeof(T);
        LoadFromExcel(excel, new ReadExcelSheet[] { loader });
        return loader.GetResults<T>();
    }
    public static void WriteExcel(string excel, params WriteExcelSheet[] writers)
    {
        using (var package = new ExcelPackage(excel))
        {
            var workbook = package.Workbook;
            // 删除之前的表
            for (int i = workbook.Worksheets.Count - 1; i >= 0; i--)
                workbook.Worksheets.Delete(i);
            foreach (var writer in writers)
            {
                if (writer.T == null)
                {
                    if (!writer.IgnoreTitle && writer.Title == null)
                        throw new ArgumentNullException("自动写入表头字段T类型不能为空");
                    if (writer.Row == null)
                        throw new ArgumentNullException("自动写入数据T类型不能为空");
                }

                var sheet = workbook.Worksheets.Add(writer.SheetName == null ? "Sheet" + (workbook.Worksheets.Count + 1) : writer.SheetName);
                int start = 1;
                // 数据列和字段名的映射
                Dictionary<int, FieldInfo> fieldMapper = new Dictionary<int, FieldInfo>();
                FieldInfo[] fields = writer.T == null ? null : writer.T.GetFields(BindingFlags.Instance | BindingFlags.Public);

                if (!writer.IgnoreTitle)
                {
                    Dictionary<string, string> title;
                    if (writer.Title == null)
                    {
                        // 根据T类型自动读取表头
                        title = new Dictionary<string, string>();
                        foreach (var item in fields)
                            title[item.Name] = item.Name;
                    }
                    else
                    {
                        title = writer.Title;
                    }

                    int column = 1;
                    foreach (var item in title)
                    {
                        fieldMapper[column] = item.Value == null ? (fields == null ? null : fields[fieldMapper.Count]) : writer.T.GetField(item.Value);
                        sheet.SetValue(start, column++, item.Key);
                    }
                    start++;
                }

                // 忽略表头时可能全是数据，并且表字段和类型字段顺序必须一致
                if (writer.Row == null && fieldMapper.Count == 0)
                {
                    int column = 1;
                    foreach (var item in fields)
                        fieldMapper[column++] = item;
                }

                start += writer.RowOffset;
                foreach (var item in writer.Results)
                {
                    var row = sheet.Rows[start];
                    try
                    {
                        if (writer.Row == null)
                        {
                            foreach (var field in fieldMapper)
                                sheet.SetValue(start, field.Key, field.Value.GetValue(item));
                        }
                        else
                        {
                            writer.Row(item, sheet.Rows[start]);
                        }
                    }
                    catch (Exception ex)
                    {
                        throw new Exception(string.Format("第{0}行数据写入错误，错误原因：{1}", row.StartRow, ex.Message));
                    }
                    start++;
                }
            }
            package.Save();
        }
    }
    public static void WriteExcel<T>(string excel, WriteExcelSheet writer)
    {
        writer.T = typeof(T);
        WriteExcel(excel, new WriteExcelSheet[] { writer });
    }
    /// <summary>获得当前行第几列的数据，列数从1开始</summary>
    public static ExcelRange Cell(this ExcelRangeRow row, int column)
    {
        // todo: 索引超出会越界
        return row.Range.Worksheet.Cells[row.StartRow, column];
    }
    public static void WriteCell(this ExcelRangeRow row, int column, object value)
    {
        row.Range.Worksheet.SetValue(row.StartRow, column, value);
    }
    static MethodInfo cellGetValue = typeof(ExcelRange).GetMethod("GetValue");
    static object[] empty = new object[0];
    public static object GetValue(this ExcelRange cell, Type type)
    {
        return cellGetValue.MakeGenericMethod(type).Invoke(cell, empty);
    }
    public static string GetTitle(this ExcelRange cell)
    {
        string value = cell.Text;
        if (string.IsNullOrEmpty(value) && cell.Merge)
        {
            cell = cell.Worksheet.Cells[cell.Worksheet.MergedCells[cell.Start.Row, cell.Start.Column]];
            value = cell.Text;
        }
        return value.Trim();
    }
}

/// <summary>NPOI插件，可以读xls和xlsx，NPOI写出来的 xlsx EPPlus 也读不了</summary>
public static class _NPOI
{
    public class ReadExcelSheet : ExcelData
    {
        /// <summary>读取一行数据，不填则默认按照字段名自动读取</summary>
        public Func<IRow, object> Row;
    }
    public class WriteExcelSheet : ExcelData
    {
        /// <summary>读取一行数据，不填则默认按照字段名自动读取</summary>
        public Action<object, IRow> Row;
    }

    /// <summary>读取Excel文件</summary>
    /// <param name="excel">Excel文件路径</param>
    /// <param name="loaders">读取表的方法</param>
    public static void LoadFromExcel(string excel, params ReadExcelSheet[] loaders)
    {
        using (var workbook = WorkbookFactory.Create(excel))
        {
            for (int i = 0; i < workbook.NumberOfSheets; i++)
            {
                var sheet = workbook.GetSheetAt(i);
                var loader = loaders.FirstOrDefault(item => item.SheetName == sheet.SheetName);
                if (loader == null)
                {
                    if (i >= loaders.Length)
                        continue;
                    loader = loaders[i];
                }

                if (loader.T == null)
                {
                    if (!loader.IgnoreTitle && loader.Title == null)
                        throw new ArgumentNullException("自动读取表头字段T类型不能为空");
                    if (loader.Row == null)
                        throw new ArgumentNullException("自动读取数据T类型不能为空");
                }

                int start = 1;
                // 数据列和字段名的映射
                Dictionary<int, FieldInfo> fieldMapper = new Dictionary<int, FieldInfo>();
                FieldInfo[] fields = loader.T == null ? null : loader.T.GetFields(BindingFlags.Instance | BindingFlags.Public);
                if (!loader.IgnoreTitle)
                {
                    Dictionary<string, string> title;
                    if (loader.Title == null)
                    {
                        // 根据T类型自动读取表头
                        title = new Dictionary<string, string>();
                        foreach (var item in fields)
                            title[item.Name] = item.Name;
                    }
                    else
                    {
                        title = loader.Title;
                    }
                    for (int l = sheet.LastRowNum; start <= l; start++)
                    {
                        var row = sheet.GetRow(start);
                        // 行没有数据会是空
                        if (row == null)
                            continue;
                        fieldMapper.Clear();
                        var titles = row.Cells.Select(i => i.StringCellValue).ToList();
                        foreach (var item in title)
                        {
                            int index = titles.IndexOf(item.Key);
                            if (index != -1)
                                fieldMapper[index] = item.Value == null ? (fields == null ? null : fields[fieldMapper.Count]) : loader.T.GetField(item.Value);
                            else
                                // 本行不是表头
                                break;
                        }
                        if (fieldMapper.Count == title.Count)
                        {
                            // 正确找到了表头
                            start++;
                            break;
                        }
                    }
                    if (fieldMapper.Count == 0)
                    {
                        throw new KeyNotFoundException("未能找到表头行：" + string.Join(",", loader.Title.Keys.ToArray()));
                    }
                }

                start += loader.RowOffset;
                loader.Results = new List<object>();
                // 忽略表头时可能全是数据，并且表字段和类型字段顺序必须一致
                if (loader.Row == null && fieldMapper.Count == 0)
                {
                    int column = 0;
                    foreach (var item in fields)
                        fieldMapper[column++] = item;
                }

                for (int j = start, l = sheet.LastRowNum; j <= l; j++)
                {
                    var row = sheet.GetRow(j);
                    if (row == null)
                        continue;
                    try
                    {

                        if (loader.Row == null)
                        {
                            object data = Activator.CreateInstance(loader.T);
                            foreach (var item in fieldMapper)
                                item.Value.SetValue(data, row.GetCell(item.Key).GetValue(item.Value.FieldType));
                            loader.Results.Add(data);
                        }
                        else
                        {
                            loader.Results.Add(loader.Row(row));
                        }
                    }
                    catch (Exception ex)
                    {
                        throw new Exception(string.Format("第{0}行数据导入错误，错误原因：{1}", row.RowNum, ex.Message));
                    }
                }
            }
        }
    }
    public static List<T> LoadFromExcel<T>(string excel, ReadExcelSheet loader)
    {
        loader.T = typeof(T);
        LoadFromExcel(excel, new ReadExcelSheet[] { loader });
        return loader.GetResults<T>();
    }
    public static void WriteExcel(string excel, params WriteExcelSheet[] writers)
    {
        //using (var package = new ExcelPackage(excel))
        //{
        //    var workbook = package.Workbook;
        //    // 删除之前的表
        //    for (int i = workbook.Worksheets.Count - 1; i >= 0; i--)
        //        workbook.Worksheets.Delete(i);
        //    foreach (var writer in writers)
        //    {
        //        if (writer.T == null)
        //        {
        //            if (!writer.IgnoreTitle && writer.Title == null)
        //                throw new ArgumentNullException("自动写入表头字段T类型不能为空");
        //            if (writer.Row == null)
        //                throw new ArgumentNullException("自动写入数据T类型不能为空");
        //        }

        //        var sheet = workbook.Worksheets.Add(writer.SheetName == null ? "Sheet" + (workbook.Worksheets.Count + 1) : writer.SheetName);
        //        int start = 1;
        //        // 数据列和字段名的映射
        //        Dictionary<int, FieldInfo> fieldMapper = new Dictionary<int, FieldInfo>();
        //        FieldInfo[] fields = writer.T == null ? null : writer.T.GetFields(BindingFlags.Instance | BindingFlags.Public);

        //        if (!writer.IgnoreTitle)
        //        {
        //            Dictionary<string, string> title;
        //            if (writer.Title == null)
        //            {
        //                // 根据T类型自动读取表头
        //                title = new Dictionary<string, string>();
        //                foreach (var item in fields)
        //                    title[item.Name] = item.Name;
        //            }
        //            else
        //            {
        //                title = writer.Title;
        //            }

        //            int column = 1;
        //            foreach (var item in title)
        //            {
        //                fieldMapper[column] = item.Value == null ? (fields == null ? null : fields[fieldMapper.Count]) : writer.T.GetField(item.Value);
        //                sheet.SetValue(start, column++, item.Key);
        //            }
        //            start++;
        //        }

        //        // 忽略表头时可能全是数据，并且表字段和类型字段顺序必须一致
        //        if (writer.Row == null && fieldMapper.Count == 0)
        //        {
        //            int column = 1;
        //            foreach (var item in fields)
        //                fieldMapper[column++] = item;
        //        }

        //        start += writer.RowOffset;
        //        foreach (var item in writer.Results)
        //        {
        //            var row = sheet.Rows[start];
        //            try
        //            {
        //                if (writer.Row == null)
        //                {
        //                    foreach (var field in fieldMapper)
        //                        sheet.SetValue(start, field.Key, field.Value.GetValue(item));
        //                }
        //                else
        //                {
        //                    writer.Row(item, sheet.Rows[start]);
        //                }
        //            }
        //            catch (Exception ex)
        //            {
        //                throw new Exception(string.Format("第{0}行数据写入错误，错误原因：{1}", row.StartRow, ex.Message));
        //            }
        //            start++;
        //        }
        //    }
        //    package.Save();
        //}
    }
    public static void WriteExcel<T>(string excel, WriteExcelSheet writer)
    {
        writer.T = typeof(T);
        WriteExcel(excel, new WriteExcelSheet[] { writer });
    }
    /// <summary>获得当前行第几列的数据，列数从1开始</summary>
    public static ICell Cell(this IRow row, int column)
    {
        return row.GetCell(column - 1);
    }
    public static void WriteCell(this IRow row, int column, object value)
    {
    }
    public static object GetValue(this ICell cell, Type type)
    {
        object value;
        if (cell.CellType == CellType.Numeric)
        {
            // NPOI中数字和日期都是NUMERIC类型的，这里对其进行判断是否是日期类型
            if (HSSFDateUtil.IsCellDateFormatted(cell))
                value = cell.DateCellValue;
            else
                value = cell.NumericCellValue;
        }
        else if (cell.CellType == CellType.Blank)
        {
            value = null;
        }
        else if (cell.CellType == CellType.Formula)
        {
            HSSFFormulaEvaluator eva = new HSSFFormulaEvaluator(cell.Sheet.Workbook);
            value = eva.Evaluate(cell).StringValue;
        }
        else
        {
            value = cell.StringCellValue;
        }

        return Convert.ChangeType(value, type);
    }
    public static string GetTitle(this ICell cell)
    {
        string value = cell.StringCellValue;
        if (string.IsNullOrEmpty(value) && cell.IsMergedCell)
        {
            for (int i = 0; i < cell.Sheet.NumMergedRegions; i++)
            {
                var range = cell.Sheet.GetMergedRegion(i);
                if (cell.ColumnIndex >= range.FirstColumn && cell.ColumnIndex <= range.LastColumn
                    && cell.RowIndex >= range.FirstRow && cell.RowIndex <= range.LastRow)
                {
                    cell = cell.Sheet.GetRow(range.FirstRow).GetCell(range.FirstColumn);
                    value = cell.StringCellValue;
                    break;
                }
            }
        }
        return value;
    }
}