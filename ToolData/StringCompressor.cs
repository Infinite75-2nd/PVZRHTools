using System;
using System.IO;
using System.IO.Compression;
using System.Text;

public static class StringCompressor
{
    /// <summary>
    /// 使用 Deflate 算法压缩字符串，并返回 Base64 编码的结果。
    /// </summary>
    /// <param name="text">要压缩的原始字符串</param>
    /// <returns>Base64 编码的压缩后字符串；若输入为 null 或空，则返回空字符串。</returns>
    public static string Compress(string text)
    {
        if (string.IsNullOrEmpty(text))
            return string.Empty;

        // 将输入字符串转为 UTF-8 字节数组
        byte[] rawBytes = Encoding.UTF8.GetBytes(text);

        using var outputStream = new MemoryStream();
        // 使用 DeflateStream 进行压缩，CompressionMode.Compress 表示压缩模式
        using (var deflateStream = new DeflateStream(outputStream, CompressionMode.Compress, leaveOpen: true))
        {
            deflateStream.Write(rawBytes, 0, rawBytes.Length);
        }

        // 从内存流中取出压缩后的字节数组
        byte[] compressedBytes = outputStream.ToArray();
        // 转为 Base64 字符串以方便文本存储/传输
        return Convert.ToBase64String(compressedBytes);
    }

    /// <summary>
    /// 解压缩由 Compress 方法生成的 Base64 字符串，还原为原始字符串。
    /// </summary>
    /// <param name="compressedBase64">压缩后的 Base64 字符串</param>
    /// <returns>解压缩后的原始字符串；若输入为 null 或空，则返回空字符串。</returns>
    /// <exception cref="FormatException">Base64 格式不正确</exception>
    /// <exception cref="InvalidDataException">压缩数据损坏或无效</exception>
    public static string Decompress(string compressedBase64)
    {
        if (string.IsNullOrEmpty(compressedBase64))
            return string.Empty;

        // 将 Base64 字符串还原为压缩字节数组
        byte[] compressedBytes = Convert.FromBase64String(compressedBase64);

        using var inputStream = new MemoryStream(compressedBytes);
        using var deflateStream = new DeflateStream(inputStream, CompressionMode.Decompress);
        using var outputStream = new MemoryStream();

        // 将解压缩后的数据复制到输出流
        deflateStream.CopyTo(outputStream);

        // 将解压后的字节数组解码为字符串
        byte[] decompressedBytes = outputStream.ToArray();
        return Encoding.UTF8.GetString(decompressedBytes);
    }
}