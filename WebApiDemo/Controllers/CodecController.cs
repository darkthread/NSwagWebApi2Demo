using System.Collections.Generic;
using System.Web.Http;
using Microsoft.AspNetCore.Mvc;
using WebApiContrib.ModelBinders;
using WebApiDemo.Models;

namespace WebApiDemo.Controllers
{
    /// <summary>
    /// 加解密功能
    /// </summary>
    [MvcStyleBinding]
    public class CodecController : ApiController
    {
        /// <summary>
        /// 加密字串
        /// </summary>
        /// <param name="encKey">加密金鑰</param>
        /// <param name="rawText">明文字串</param>
        /// <returns>加密字串</returns>
        [HttpPost, Route("api/codec/EncryptString")]
        [Consumes("application/x-www-form-urlencoded")]
        public byte[] EncryptString([FromForm]string encKey, [FromForm]string rawText)
        {
            return CodecModule.EncrytString(encKey, rawText);
        }

        /// <summary>
        /// 解密請求參數物件
        /// </summary>
        public class DecryptParameter
        {
            /// <summary>
            /// 加密金鑰
            /// </summary>
            public string EncKey { get; set; }
            /// <summary>
            /// 加密字串陣列
            /// </summary>
            public List<byte[]> EncData { get; set; }
        }

        /// <summary>
        /// 批次解密
        /// </summary>
        /// <param name="decData">解密請求參數(加解密金鑰與加密字串陣列)</param>
        /// <returns>解密字串陣列</returns>
        [HttpPost]
        public List<string> BatchDecryptData([FromBody]DecryptParameter decData)
        {
            return CodecModule.DecryptData(decData.EncKey, decData.EncData);
        }
    }
}
