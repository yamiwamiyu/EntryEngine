using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using EntryEngine;
using EntryEngine.Network;

public static class _EX
{
    /// <summary>SQL语句LIKE后使用的字符串参数，例如LIKE '李%'，就是'李'.Like(false, true)</summary>
    /// <param name="key">搜索的关键字</param>
    /// <param name="left">开头是否使用'%'通配符</param>
    /// <param name="right">结尾是否使用'%'通配符</param>
    public static string Like(this string key, bool left = true, bool right = true)
    {
        if (string.IsNullOrEmpty(key)) return null;
        if (left) key = "%" + key;
        if (right) key = key + "%";
        return key;
    }
    /// <summary>SQL语句IN后的参数(SQL 语句需要自带括号，例如 IN (@p0))，数组转换成1,2,3</summary>
    public static string In<T>(this T[] key)
    {
        return string.Join(",", key);
    }
}
/// <summary>上传/下载文件通用</summary>
public static class _FILE
{
    /// <summary>本地资源访问的路径</summary>
    public static string AccessURL;

    public const string NATIVE_UPLOAD_PATH = "temp/";

    public static void CheckPath(string path)
    {
        if (!string.IsNullOrEmpty(path) && !Directory.Exists(path))
            Directory.CreateDirectory(path);
    }
    public static void CheckFilePath(string nativeFileName)
    {
        CheckPath(Path.GetDirectoryName(nativeFileName));
    }
    public static void DeleteFile(string key)
    {
        _LOG.Info("删除本地文件：{0}", key);
        if (File.Exists(key)) File.Delete(key);
    }
    public static void DeleteFile(string[] keys)
    {
        if (keys == null) return;
        for (int i = 0; i < keys.Length; i++)
            DeleteFile(keys[i]);
    }
    public static void DeleteDirectory(string relativePath)
    {
        _LOG.Info("删除本地目录：{0}", relativePath);
        if (Directory.Exists(relativePath))
            Directory.Delete(relativePath, true);
    }

    /// <summary>在修改上传的图片时，若是完整URL则不需要修改</summary>
    public static bool IsFullURL(this string url)
    {
        if (string.IsNullOrEmpty(url))
            return false;

        if (!string.IsNullOrEmpty(AccessURL))
            return url.StartsWith(AccessURL);

        // 网站和图片属于同一个源的情况下使用相对路径
        return File.Exists(url);
    }
    public static void ResolveImage(this string image, out string imageFull)
    {
        if (string.IsNullOrEmpty(image))
            imageFull = string.Empty;
        else
            imageFull = string.Format("{0}{1}", AccessURL, image);
    }
    public static void ResolveImage(this string[] image, out string[] imageFull)
    {
        if (image == null)
            image = new string[0];
        imageFull = new string[image.Length];
        for (int i = 0; i < image.Length; i++)
            image[i].ResolveImage(out imageFull[i]);
    }
    public static void CheckFileTypeCanEmpty(this string filename, params string[] suffix)
    {
        if (!string.IsNullOrEmpty(filename))
            CheckFileType(filename, suffix);
    }
    public static void CheckFileTypeCanEmpty(this string[] filenames, params string[] suffix)
    {
        if (filenames != null)
        {
            foreach (var item in filenames)
            {
                CheckFileType(item, suffix);
            }
        }
    }
    /// <summary>检查文件类型</summary>
    /// <param name="suffix">包含'.'</param>
    public static void CheckFileType(this string filename, params string[] suffix)
    {
        string extension = Path.GetExtension(filename);
        "不支持的文件格式".Check(suffix.Length > 0 && !suffix.Any(s => s == extension));
    }

    /// <summary>将上传的文件写入临时文件夹暂存</summary>
    /// <param name="file">上传的文件</param>
    /// <returns>返回临时文件名（不带临时文件夹路径，带后缀）</returns>
    public static string WriteUploadFile(FileUpload file, bool SaveOriginName = false)
    {
        CheckPath(NATIVE_UPLOAD_PATH);
        string fileName = Guid.NewGuid().ToString("n");
        if (SaveOriginName)
            fileName += ("_" + Path.GetFileName(file.Filename));
        else
            fileName += Path.GetExtension(file.Filename);
        file.SaveAs(NATIVE_UPLOAD_PATH + fileName);
        return fileName;
    }
    /// <summary>拷贝uploadFile到targetFileName，删除uploadFile的文件</summary>
    /// <param name="uploadFile">WriteUploadFile返回的路径</param>
    /// <param name="targetFileName">要保存的目标路径</param>
    private static void SaveUploadFile(string uploadFile, ref string targetFileName)
    {
        if (string.IsNullOrEmpty(Path.GetFileNameWithoutExtension(targetFileName)))
            throw new ArgumentException("保存的文件名不能为空");

        // 最后要保存的文件
        if (string.IsNullOrEmpty(Path.GetExtension(targetFileName)))
        {
            // 自动添加后缀
            string extension = Path.GetExtension(uploadFile);
            if (!string.IsNullOrEmpty(extension))
                targetFileName += extension;
        }

        if (uploadFile != targetFileName)
        {
            CheckFilePath(targetFileName);
            // 写入本地
            _LOG.Debug("本地保存上传的文件：{0} - {1}", uploadFile, targetFileName);
            File.Copy(uploadFile, targetFileName, true);
            if (File.Exists(uploadFile))
                File.Delete(uploadFile);
        }
    }
    /// <summary>图片上传</summary>
    /// <param name="oldFile">之前已经上传过的文件路径，若上传了新文件，此路径将变为新路径</param>
    /// <param name="upload">本次上传的图片路径，有可能没有变化</param>
    /// <param name="newFile">本次要保存的图片目标路径</param>
    public static void SaveUploadFile(ref string oldFile, ref string upload, string newFile, bool isDeleteOld = true)
    {
        bool isOldEmpty = string.IsNullOrEmpty(oldFile);
        bool isNewEmpty = string.IsNullOrEmpty(upload);

        if (!isNewEmpty)
        {
            // 上传新文件 | 新旧文件不一样替换文件
            if (oldFile != upload)
            {
                if (isDeleteOld && !isOldEmpty)
                {
                    // 新旧文件不一样，删除旧文件
                    DeleteFile(oldFile);
                }

                // 上传新文件
                SaveUploadFile(NATIVE_UPLOAD_PATH + upload, ref newFile);
                oldFile = newFile;
                upload = newFile;
            }
        }
        else
        {
            if (isDeleteOld && !isOldEmpty)
            {
                // 删除旧文件
                DeleteFile(oldFile);
                oldFile = null;
            }
            // 都为null，不做任何操作
        }
    }
    /// <summary>图片上传</summary>
    /// <param name="upload">本次上传的图片路径</param>
    /// <param name="newFile">本次要保存的图片目标路径</param>
    public static void SaveUploadFile(ref string upload, string newFile)
    {
        // 上传新文件
        SaveUploadFile(NATIVE_UPLOAD_PATH + upload, ref newFile);
        upload = newFile;
    }
    public static void SaveUploadFile(ref string[] oldFiles, ref string[] uploads, string[] newFiles, bool isDeleteOld = true)
    {
        if (oldFiles == null)
            oldFiles = new string[0];
        if (uploads == null)
            uploads = new string[0];
        if (newFiles == null)
            newFiles = new string[0];

        if (uploads.Length != newFiles.Length)
            throw new ArgumentException("上传的文件和相对应的文件名数组长度应该一样");

        // 需要删除的文件
        List<string> delete = new List<string>();

        // 文件已经不存在，需要删除
        if (isDeleteOld)
            for (int i = 0; i < oldFiles.Length; i++)
                if (!uploads.Contains(oldFiles[i]))
                    DeleteFile(oldFiles[i]);

        // 新文件，需要重新上传
        for (int i = 0; i < uploads.Length; i++)
        {
            int index = oldFiles.IndexOf(uploads[i]);
            if (index == -1)
                // 新文件
                SaveUploadFile(NATIVE_UPLOAD_PATH + uploads[i], ref newFiles[i]);
            //_LOG.Debug("新文件：{0}", newFiles[i]);
            else
                // 没有变化的文件
                newFiles[i] = oldFiles[index];
            //_LOG.Debug("没有变化的文件：{0}", uploads[i]);
        }

        oldFiles = newFiles;
        uploads = newFiles;
    }
    public static void SaveUploadFile(ref string[] oldFiles, ref string[] uploads, string dir, bool isDeleteOld = true)
    {
        int len = uploads == null ? 0 : uploads.Length;
        string[] newFiles = new string[len];
        for (int i = 0; i < len; i++)
            newFiles[i] = dir + uploads[i];
        SaveUploadFile(ref oldFiles, ref uploads, newFiles, isDeleteOld);
    }
    public static void SaveUploadFile(ref string[] uploads, string dir)
    {
        int len = uploads == null ? 0 : uploads.Length;
        string[] newFiles = new string[len];
        for (int i = 0; i < len; i++)
        {
            newFiles[i] = dir + uploads[i];
            SaveUploadFile(NATIVE_UPLOAD_PATH + uploads[i], ref newFiles[i]);
            uploads[i] = newFiles[i];
        }
    }
}