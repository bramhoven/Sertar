﻿namespace Poseidon.API.Models.RequestData
{
    public class ServerCreateData
    {
        #region Properties

        public string Image { get; set; }
        public string Name { get; set; }
        public string Region { get; set; }
        public string Size { get; set; }
        public string SshKeyId { get; set; }

        #endregion
    }
}