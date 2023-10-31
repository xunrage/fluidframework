using System;

namespace FluidFramework.Context
{
    /// <summary>
    /// Stores information for an user.
    /// </summary>
    public class User
    {
        /// <summary>
        /// The identifier of the user.
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// The account of the user.
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// The password of the user.
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// The full name of the user.
        /// </summary>
        public string FullName { get; set; }

        /// <summary>
        /// The sex of the user.
        /// </summary>
        public string Sex { get; set; }

        /// <summary>
        /// The work station of the user.
        /// </summary>
        public string WorkStation { get; set; }
    }
}
