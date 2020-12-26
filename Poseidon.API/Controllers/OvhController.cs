﻿using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using NLog;
using Poseidon.Api.Models.RequestData;
using Poseidon.API.Models.RequestData;
using Poseidon.Models.Cloud;
using Poseidon.Models.Servers;
using Poseidon.BusinessLayer.Cloud;
using Poseidon.BusinessLayer.Servers;
using Poseidon.DataLayer.Cloud;
using Poseidon.DataLayer.Servers;

namespace Poseidon.Api.Controllers
{
    [Route("api/ovh")]
    [ApiController]
    public class OvhController : ControllerBase
    {
        #region Fields

        private readonly CloudManager _cloudManager;

        private readonly ServerManager _serverManager;

        #endregion

        public ILogger Logger { get; } = LogManager.GetCurrentClassLogger();

        #region Constructors

        public OvhController(IServerDal serverDal)
        {
            _cloudManager = new CloudManager(new OvhCloudDal());
            _serverManager = new ServerManager(serverDal);
        }

        #endregion

        #region Methods

        /// <summary>
        ///     Create a server in the ovh cloud.
        /// </summary>
        /// <param name="serverCreateData">The server data</param>
        /// <returns></returns>
        [Route("servers")]
        [HttpPost]
        public ActionResult<object> CreateServer(ServerCreateData serverCreateData)
        {
            try
            {
                var server = _cloudManager.CreateServer(serverCreateData.Name, serverCreateData.Size,
                    serverCreateData.Image,
                    serverCreateData.Region);
                if (server != null)
                {
                    _serverManager.InsertServer(server);
                    return Ok(server);
                }
            }
            catch (Exception e)
            {
                Logger.Error(e);
            }

            return BadRequest(new {Message = "Failed to create server"});
        }

        /// <summary>
        ///     Getting ovh's available images.
        /// </summary>
        /// <returns></returns>
        [Route("images")]
        [HttpGet]
        public ActionResult<ICollection<InstanceSizeBase>> GetAvailableImages()
        {
            return Ok(_cloudManager.GetAvailableImages());
        }

        /// <summary>
        ///     Getting ovh's available images by region.
        /// </summary>
        /// <param name="region">The region to filter by</param>
        /// <returns></returns>
        [Route("images/regions/{region}")]
        [HttpGet]
        public ActionResult<ICollection<InstanceSizeBase>> GetAvailableImages(string region)
        {
            return Ok(_cloudManager.GetAvailableImages(region));
        }

        /// <summary>
        ///     Getting ovh's available sizes.
        /// </summary>
        /// <returns></returns>
        [Route("sizes")]
        [HttpGet]
        public ActionResult<ICollection<InstanceSizeBase>> GetAvailableSizes()
        {
            return Ok(_cloudManager.GetAvailableSizes());
        }

        /// <summary>
        ///     Getting ovh's available sizes by region.
        /// </summary>
        /// <param name="region">The region to filter by</param>
        /// <returns></returns>
        [Route("sizes/regions/{region}")]
        [HttpGet]
        public ActionResult<ICollection<InstanceSizeBase>> GetAvailableSizesForRegion(string region)
        {
            return Ok(_cloudManager.GetAvailableSizes(region));
        }

        /// <summary>
        ///     Gets a server in the ovh cloud.
        /// </summary>
        /// <param name="serverId">The server id</param>
        /// <returns></returns>
        [Route("servers/{serverId}")]
        [HttpGet]
        public ActionResult<object> GetServer(string serverId)
        {
            try
            {
                var server = _cloudManager.GetServer(serverId);
                if (server != null)
                    return Ok(server);
            }
            catch (Exception e)
            {
                Logger.Error(e);
            }

            return BadRequest(new {Message = "Failed to get server"});
        }

        /// <summary>
        ///     Updates a server in the ovh cloud.
        /// </summary>
        /// <param name="serverUpdateData">The server data to update</param>
        /// <returns></returns>
        [Route("servers")]
        [HttpPut]
        public ActionResult<object> UpdateServer(ServerUpdateData serverUpdateData)
        {
            if (serverUpdateData == null)
                return BadRequest(new {Message = "No data provided"});

            if (string.IsNullOrWhiteSpace(serverUpdateData.CloudId) &&
                (serverUpdateData.Id == null || string.IsNullOrWhiteSpace(serverUpdateData.Id)))
                return BadRequest(new {Message = "No id has been provided"});

            try
            {
                if (string.IsNullOrWhiteSpace(serverUpdateData.CloudId))
                    serverUpdateData.CloudId = _serverManager.GetServer(Guid.Parse(serverUpdateData.Id)).CloudId;

                var updatedServer = _cloudManager.UpdateServer(new Server
                    {CloudId = serverUpdateData.CloudId, Name = serverUpdateData.Name});
                if (updatedServer != null)
                    return Ok(updatedServer);
            }
            catch (Exception e)
            {
                Logger.Error(e);
            }

            return BadRequest(new {Message = "Failed to update server"});
        }

        #endregion
    }
}