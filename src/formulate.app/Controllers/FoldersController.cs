﻿namespace formulate.app.Controllers
{

    // Namespaces.
    using Folders;
    using Helpers;
    using Models.Requests;
    using Persistence;
    using Resolvers;
    using System;
    using System.Linq;
    using System.Web.Http;
    using Umbraco.Core.Logging;
    using Umbraco.Web;
    using Umbraco.Web.Editors;
    using Umbraco.Web.Mvc;
    using Umbraco.Web.WebApi.Filters;
    using CoreConstants = Umbraco.Core.Constants;


    /// <summary>
    /// Controller for Formulate forms.
    /// </summary>
    [PluginController("formulate")]
    [UmbracoApplicationAuthorize(CoreConstants.Applications.Users)]
    public class FoldersController : UmbracoAuthorizedJsonController
    {

        #region Constants

        private const string UnhandledError = @"An unhandled error occurred. Refer to the error log.";
        private const string CreateFolderError = @"An error occurred while attempting to create the Formulate folder.";

        #endregion


        #region Properties

        private IFolderPersistence Persistence { get; set; }
        private IEntityPersistence Entities { get; set; }

        #endregion


        #region Constructors

        /// <summary>
        /// Default constructor.
        /// </summary>
        public FoldersController()
            : this(UmbracoContext.Current)
        {
        }


        /// <summary>
        /// Primary constructor.
        /// </summary>
        /// <param name="context">Umbraco context.</param>
        public FoldersController(UmbracoContext context)
            : base(context)
        {
            Persistence = FolderPersistence.Current.Manager;
            Entities = EntityPersistence.Current.Manager;
        }

        #endregion


        #region Web Methods

        /// <summary>
        /// Persists a folder.
        /// </summary>
        /// <param name="request">
        /// The request to persist a folder.
        /// </param>
        /// <returns>
        /// An object indicating success or failure, along with some
        /// folder data.
        /// </returns>
        [HttpPost]
        public object PersistFolder(PersistFolderRequest request)
        {

            // Variables.
            var result = default(object);
            var folderId = Guid.NewGuid();


            // Catch all errors.
            try
            {

                // Get path.
                var parentId = GuidHelper.GetGuid(request.ParentId);
                var parent = Entities.Retrieve(parentId);
                var path = parent.Path.Concat(new[] { folderId }).ToArray();


                // Create the folder.
                var folder = new Folder()
                {
                    Id = folderId,
                    Path = path,
                    Name = request.FolderName
                };


                // Persist the folder.
                Persistence.Persist(folder);


                // Success.
                result = new
                {
                    Success = true,
                    FolderId = GuidHelper.GetString(folderId),
                    Path = path.Select(x => GuidHelper.GetString(x))
                        .ToArray()
                };

            }
            catch (Exception ex)
            {

                // Error.
                LogHelper.Error<FoldersController>(CreateFolderError, ex);
                result = new
                {
                    Success = false,
                    Reason = UnhandledError
                };

            }


            // Return the result.
            return result;

        }

        #endregion

    }

}