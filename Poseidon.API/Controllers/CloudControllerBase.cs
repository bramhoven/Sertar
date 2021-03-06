﻿using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using NLog;
using Poseidon.Api.Models;
using Poseidon.Api.Models.RequestData;
using Poseidon.API.Models.RequestData;
using Poseidon.BusinessLayer.Cloud;
using Poseidon.BusinessLayer.Servers;
using Poseidon.DataLayer.Cloud;
using Poseidon.DataLayer.Servers;
using Poseidon.Models.Cloud;
using Poseidon.Models.Servers;

namespace Poseidon.Api.Controllers
{
    public class CloudControllerBase : ControllerBase
    {
        #region Fields

        protected readonly CloudManager CloudManager;

        protected readonly ServerManager ServerManager;

        #endregion

        #region Properties

        public ILogger Logger { get; } = LogManager.GetCurrentClassLogger();

        #endregion

        #region Constructors

        public CloudControllerBase(IServerDal serverDal, ICloudDal cloudDal, ICloudProviderDal cloudProviderDal)
        {
            ServerManager = new ServerManager(serverDal, cloudProviderDal);
            cloudDal.ConfigureProvider(cloudProviderDal);
            CloudManager = new CloudManager(cloudDal);
        }

        #endregion

        #region Methods

        /// <summary>
        ///     Create a server.
        /// </summary>
        /// <param name="serverCreateData">The server data</param>
        /// <returns></returns>
        [Route("servers")]
        [HttpPost]
        public ActionResult<object> CreateServer(ServerCreateData serverCreateData)
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(serverCreateData.Name))
                errors.Add($"Attribute {nameof(serverCreateData.Name)} is required");

            if (string.IsNullOrWhiteSpace(serverCreateData.Size))
                errors.Add($"Attribute {nameof(serverCreateData.Size)} is required");

            if (string.IsNullOrWhiteSpace(serverCreateData.Image))
                errors.Add($"Attribute {nameof(serverCreateData.Image)} is required");

            if (string.IsNullOrWhiteSpace(serverCreateData.Region))
                errors.Add($"Attribute {nameof(serverCreateData.Region)} is required");

            if (string.IsNullOrWhiteSpace(serverCreateData.SshKeyId))
                errors.Add($"Attribute {nameof(serverCreateData.SshKeyId)} is required");

            try
            {
                var server = CloudManager.CreateServer(serverCreateData.Name, serverCreateData.Size,
                    serverCreateData.Image,
                    serverCreateData.Region, serverCreateData.SshKeyId);
                if (server != null)
                {
                    if (ServerManager.InsertServer(server))
                        return Ok(server);
                }
            }
            catch (Exception e)
            {
                Logger.Error(e);
            }

            return BadRequest(new ErrorMessage("Failed to create server", errors));
        }

        /// <summary>
        ///     Deletes a server.
        /// </summary>
        /// <param name="cloudId">The server id</param>
        /// <returns></returns>
        [Route("servers/{cloudId}")]
        [HttpDelete]
        public ActionResult<object> DeleteServer(string cloudId)
        {
            try
            {
                var server = ServerManager.GetServerByCloudId(cloudId);

                if (server == null)
                    return BadRequest(new ErrorMessage($"Failed to delete server with cloud id: {cloudId}"));

                if (ServerManager.DeleteServer(cloudId))
                    return Ok();
            }
            catch (Exception e)
            {
                Logger.Error(e);
            }

            return BadRequest(new {Message = "Failed to delete server"});
        }

        /// <summary>
        ///     Gets the public keys at provider.
        /// </summary>
        /// <returns></returns>
        [Route("sshkeys")]
        [HttpGet]
        public ActionResult<object> GetAllSshKeys()
        {
            try
            {
                return Ok(CloudManager.GetSshKeys());
            }
            catch (Exception e)
            {
                Logger.Error(e);
            }

            return BadRequest(new ErrorMessage("Failed to update server"));
        }

        /// <summary>
        ///     Getting the available images.
        /// </summary>
        /// <returns></returns>
        [Route("images")]
        [HttpGet]
        public ActionResult<ICollection<InstanceSizeBase>> GetAvailableImages()
        {
            return Ok(CloudManager.GetAvailableImages());
        }

        /// <summary>
        ///     Getting the available images by region.
        /// </summary>
        /// <param name="region">The region to filter by</param>
        /// <returns></returns>
        [Route("images/regions/{region}")]
        [HttpGet]
        public ActionResult<ICollection<InstanceSizeBase>> GetAvailableImages(string region)
        {
            return Ok(CloudManager.GetAvailableImages(region));
        }

        /// <summary>
        ///     Getting the available sizes.
        /// </summary>
        /// <returns></returns>
        [Route("sizes")]
        [HttpGet]
        public ActionResult<ICollection<InstanceSizeBase>> GetAvailableSizes()
        {
            return Ok(CloudManager.GetAvailableSizes());
        }

        /// <summary>
        ///     Getting the available sizes by region.
        /// </summary>
        /// <param name="region">The region to filter by</param>
        /// <returns></returns>
        [Route("sizes/regions/{region}")]
        [HttpGet]
        public ActionResult<ICollection<InstanceSizeBase>> GetAvailableSizesForRegion(string region)
        {
            return Ok(CloudManager.GetAvailableSizes(region));
        }

        /// <summary>
        ///     Getting all the regions.
        /// </summary>
        /// <returns></returns>
        [Route("regions")]
        [HttpGet]
        public ActionResult<ICollection<InstanceSizeBase>> GetRegions()
        {
            return Ok(CloudManager.GetRegions());
        }

        /// <summary>
        ///     Gets a server.
        /// </summary>
        /// <param name="cloudId">The server id</param>
        /// <returns></returns>
        [Route("servers/{cloudId}")]
        [HttpGet]
        public ActionResult<object> GetServer(string cloudId)
        {
            try
            {
                var server = CloudManager.GetServer(cloudId);
                if (server != null)
                    return Ok(server);
            }
            catch (Exception e)
            {
                Logger.Error(e);
            }

            return BadRequest(new ErrorMessage("Failed to get server"));
        }

        /// <summary>
        ///     Updates a server.
        /// </summary>
        /// <param name="serverUpdateData">The server data to update</param>
        /// <returns></returns>
        [Route("servers")]
        [HttpPut]
        public ActionResult<object> UpdateServer(ServerUpdateData serverUpdateData)
        {
            if (serverUpdateData == null)
                return BadRequest(new ErrorMessage("No data provided"));

            if (string.IsNullOrWhiteSpace(serverUpdateData.CloudId) &&
                (serverUpdateData.Id == null || string.IsNullOrWhiteSpace(serverUpdateData.Id)))
                return BadRequest(new ErrorMessage("No id has been provided"));

            try
            {
                if (string.IsNullOrWhiteSpace(serverUpdateData.CloudId))
                    serverUpdateData.CloudId = ServerManager.GetServer(Guid.Parse(serverUpdateData.Id)).CloudId;

                var updatedServer = CloudManager.UpdateServer(new Server
                    {CloudId = serverUpdateData.CloudId, Name = serverUpdateData.Name});
                if (updatedServer != null)
                    return Ok(updatedServer);
            }
            catch (Exception e)
            {
                Logger.Error(e);
            }

            return BadRequest(new ErrorMessage("Failed to update server"));
        }

        #endregion
    }
}