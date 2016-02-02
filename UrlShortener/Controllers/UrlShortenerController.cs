using Common;
using Common.Exceptions;
using log4net;
using System;
using System.Web.Configuration;
using System.Web.Mvc;
using UrlShortener.Models;

namespace UrlShortener.Controllers
{
    /// <summary>
    /// Controller for MVC pattern
    /// </summary>
    public class UrlShortenerController : Controller
    {
        private readonly UrlServices.UrlShortenerService _urlShortenerService;
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private string _shortUrlPrefix;

        public UrlShortenerController()
        {
            string dbConnString = WebConfigurationManager.AppSettings["DBConnectionString"];
            string cacheConnString = WebConfigurationManager.AppSettings["CacheConnectionString"];
            string cacheMode = WebConfigurationManager.AppSettings["CacheMode"];
            _urlShortenerService = new UrlServices.UrlShortenerService(dbConnString, cacheConnString, cacheMode);
            _shortUrlPrefix = WebConfigurationManager.AppSettings["ShortUrlPrefix"];

        }

        /// <summary>
        /// Expands the URL.
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        public ActionResult ExpandUrl()
        {
            return View();
        }

        /// <summary>
        /// Calls UrlShortenerService to Expand the URL.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns></returns>
        [HttpPost]
        [AllowAnonymous]
        public ActionResult ExpandUrl(ExpandUrlModel model)
        {
            try
            {
                string cleanedShortUrl = null;
                if (!Utilities.ValidateShortUrl(model.ShortUrl, this._shortUrlPrefix, out cleanedShortUrl))
                {
                    ModelState.AddModelError("", ErrorCodeToString(UrlServicesStatus.InvalidShortUrl) + this._shortUrlPrefix + "{ShortUrl}");
                    return View(model);
                }

                var fullUrl = _urlShortenerService.ExpandUrl(cleanedShortUrl);
                if (!string.IsNullOrEmpty(fullUrl))
                {
                    ViewBag.StatusMessage = fullUrl;
                }
                else
                {
                    ModelState.AddModelError("", ErrorCodeToString(UrlServicesStatus.ErrorFullUrlNotFound));
                    Log.Warn("Full Url Not Found for Short Url : " + cleanedShortUrl);
                }
            }
            catch (Exception e)
            {
                ModelState.AddModelError("", ErrorCodeToString(UrlServicesStatus.UnknownError));
                Log.Error(ErrorCodeToString(UrlServicesStatus.UnknownError), e);
            }

            return View(model);
        }

        /// <summary>
        /// Shortens the URL.
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        public ActionResult ShortenUrl()
        {
            return View();
        }

        /// <summary>
        /// Calls UrlShortenerService to Shorten the URL.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns></returns>
        [HttpPost]
        [AllowAnonymous]
        public ActionResult ShortenUrl(ShortenUrlModel model)
        {
            try
            {
                string cleanedFullUrl = null;
                if (!Utilities.ValidateFullUrl(model.FullUrl, out cleanedFullUrl))
                {
                    ModelState.AddModelError("", ErrorCodeToString(UrlServicesStatus.InvalidFullUrl));
                    return View(model);
                }
                var shortUrl = _urlShortenerService.ShortenUrl(cleanedFullUrl);
                if (!string.IsNullOrEmpty(shortUrl))
                {
                    ViewBag.StatusMessage = _shortUrlPrefix + shortUrl;
                }
                else
                {
                    ModelState.AddModelError("", ErrorCodeToString(UrlServicesStatus.ErrorShortUrlCreation));
                }

            }
            catch (ShortUrlHashSlotNotAvailableException e)
            {
                ModelState.AddModelError("", ErrorCodeToString(UrlServicesStatus.ErrorShortUrlCreation));
                Log.Fatal("Empty Url Hash Slot could not be found", e);                
            }
            catch (Exception e)
            {
                ModelState.AddModelError("", ErrorCodeToString(UrlServicesStatus.UnknownError));
                Log.Error(ErrorCodeToString(UrlServicesStatus.UnknownError), e);
            }

            return View(model);
        }

        #region Helpers

        private static string ErrorCodeToString(UrlServicesStatus status)
        {
            switch (status)
            {
                case UrlServicesStatus.InvalidFullUrl:
                    return "Full Url is not valid url.";

                case UrlServicesStatus.ErrorFullUrlNotFound:
                    return "Full Url Could not be found";

                case UrlServicesStatus.ErrorShortUrlCreation:
                    return "Short Url Could not be created. Please try again. If the problem persists, please contact site admin.";

                case UrlServicesStatus.InvalidShortUrl:
                    return "Short Url is not valid. Expect Url of the form : ";
                    
                case UrlServicesStatus.UnknownError:
                    return "An unknown error occurred. Please try again. If the problem persists, please contact site admin.";

                default:
                    return "An unknown error occurred. Please try again. If the problem persists, please contact site admin.";
            }
        }
        #endregion
    }
}
