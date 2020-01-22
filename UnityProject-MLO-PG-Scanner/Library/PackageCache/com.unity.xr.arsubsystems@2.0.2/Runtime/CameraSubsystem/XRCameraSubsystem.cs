using System;
using System.Collections.Generic;
using Unity.Collections;

namespace UnityEngine.XR.ARSubsystems
{
    /// <summary>
    /// Provides access to a device's camera.
    /// </summary>
    /// <remarks>
    /// The <c>XRCameraSubsystem</c> links a Unity <c>Camera</c> to a device camera for video overlay (pass-thru
    /// rendering). It also allows developers to query for environmental light estimation, when available.
    /// </remarks>
    public abstract class XRCameraSubsystem : Subsystem<XRCameraSubsystemDescriptor>
    {
        /// <summary>
        /// Indicates whether this subsystem is running (i.e., <see cref="Start"/>
        /// has been called).
        /// </summary>
        public override bool running
        {
            get { return m_Running; }
        }

        /// <summary>
        /// The provider created by the implementation that contains the required camera functionality.
        /// </summary>
        /// <value>
        /// The provider created by the implementation that contains the required camera functionality.
        /// </value>
        IProvider m_Provider;

        /// <summary>
        /// Construct the <c>XRCameraSubsystem</c>.
        /// </summary>
        public XRCameraSubsystem()
        {
            m_Provider = CreateProvider();
            Debug.Assert(m_Provider != null, "camera functionality provider cannot be null");
        }

        /// <summary>
        /// Interface for providing camera functionality for the implementation.
        /// </summary>
        protected class IProvider
        {
            /// <summary>
            /// Method to be implemented by provider to start the camera for the subsystem.
            /// </summary>
            public virtual void Start()
            { }

            /// <summary>
            /// Method to be implemented by provider to stop the camera for the subsystem.
            /// </summary>
            public virtual void Stop()
            { }

            /// <summary>
            /// Method to be implemented by provider to destroy the camera for the subsystem.
            /// </summary>
            public virtual void Destroy()
            { }

            /// <summary>
            /// Method to be implemented by provider to get the camera frame for the subsystem.
            /// </summary>
            /// <param name="cameraParams">The current Unity <c>Camera</c> parameters.</param>
            /// <param name="cameraFrame">The current camera frame returned by the method.</param>
            /// <returns>
            /// <c>true</c> if the method successfully got a frame. Otherwise, <c>false</c>.
            /// </returns>
            public virtual bool TryGetFrame(
                XRCameraParams cameraParams,
                out XRCameraFrame cameraFrame)
            {
                cameraFrame = default(XRCameraFrame);
                return false;
            }

            /// <summary>
            /// Method to be implemented by the provder to get the shader name used by <c>XRCameraSubsystem</c> to render
            /// texture.
            /// </summary>
            public virtual string shaderName
            {
                get { return null; }
            }

            /// <summary>
            /// Method to be implemented by the provider to set the focus mode for the camera.
            /// </summary>
            /// <param name="cameraFocusMode">The focus mode to set for the camera.</param>
            /// <returns>
            /// <c>true</c> if the method successfully set the focus mode for the camera. Otherwise, <c>false</c>.
            /// </returns>
            public virtual bool TrySetFocusMode(
                CameraFocusMode cameraFocusMode)
            {
                return false;
            }

            /// <summary>
            /// Method to be implemented by the provider to set the light estimation mode.
            /// </summary>
            /// <param name="lightEstimationMode">The light estimation mode to set.</param>
            /// <returns>
            /// <c>true</c> if the method successfully set the light estimation mode. Otherwise, <c>false</c>.
            /// </returns>
            public virtual bool TrySetLightEstimationMode(
                LightEstimationMode lightEstimationMode)
            {
                return false;
            }

            /// <summary>
            /// Method to be implemented by the provider to get the camera intrinisics information.
            /// </summary>
            /// <param name="cameraIntrinsics">The camera intrinsics information returned from the method.</param>
            /// <returns>
            /// <c>true</c> if the method successfully gets the camera intrinsics information. Otherwise, <c>false</c>.
            /// </returns>
            public virtual bool TryGetIntrinsics(
                out XRCameraIntrinsics cameraIntrinsics)
            {
                cameraIntrinsics = default(XRCameraIntrinsics);
                return false;
            }

            /// <summary>
            /// Method to be implemented by the provider to determine whether camera permission has been granted.
            /// </summary>
            public virtual bool permissionGranted
            {
                get { return false; }
            }

            /// <summary>
            /// Get the <see cref="XRTextureDescriptor"/>s associated with the current
            /// <see cref="XRCameraFrame"/>.
            /// </summary>
            /// <returns>The current texture descriptors.</returns>
            /// <param name="defaultDescriptor">A default value which should
            /// be used to fill the returned array before copying in the 
            /// real values. This ensures future additions to this struct
            /// are backwards compatible.</param>
            /// <param name="allocator">The allocator to use when creating
            /// the returned <c>NativeArray</c>.</param>
            public virtual NativeArray<XRTextureDescriptor> GetTextureDescriptors(
                XRTextureDescriptor defaultDescriptor,
                Allocator allocator)
            {
                return new NativeArray<XRTextureDescriptor>(0, allocator);
            }
        }

        /// <summary>
        /// Specifies the focus mode for the camera.
        /// </summary>
        /// <value>
        /// The focus mode for the camera.
        /// </value>
        public CameraFocusMode focusMode
        {
            get { return m_FocusMode; }
            set
            {
                if ((m_FocusMode != value) && m_Provider.TrySetFocusMode(value))
                {
                    m_FocusMode = value;
                }
            }
        }
        CameraFocusMode m_FocusMode = CameraFocusMode.Fixed;

        /// <summary>
        /// Specifies the light estimation mode.
        /// </summary>
        /// <value>
        /// The light estimation mode.
        /// </value>
        public LightEstimationMode lightEstimationMode
        {
            get { return m_LightEstimationMode; }
            set
            {
                if ((m_LightEstimationMode != value) && m_Provider.TrySetLightEstimationMode(value))
                {
                    m_LightEstimationMode = value;
                }
            }
        }
        LightEstimationMode m_LightEstimationMode = LightEstimationMode.Disabled;

        bool m_Running;

        /// <summary>
        /// Start the camera subsystem.
        /// </summary>
        public sealed override void Start()
        {
            if (!m_Running)
            {
                m_Provider.Start();
            }

            m_Running = true;
        }

        /// <summary>
        /// Stop the camera subsystem.
        /// </summary>
        public sealed override void Stop()
        {
            if (m_Running)
            {
                m_Provider.Stop();
            }

            m_Running = false;
        }

        /// <summary>
        /// Destroy the camera subsystem.
        /// </summary>
        public sealed override void Destroy()
        {
            Stop();
            m_Provider.Destroy();
        }

        /// <summary>
        /// Gets the <see cref="XRTextureDescriptor"/>s associated with the
        /// current frame. The caller owns the returned <c>NativeArray</c>
        /// and is responsible for calling <c>Dispose</c> on it.
        /// </summary>
        /// <returns>An array of texture descriptors.</returns>
        /// <param name="allocator">The allocator to use when creating
        /// the returned <c>NativeArray</c>.</param>
        public NativeArray<XRTextureDescriptor> GetTextureDescriptors(
            Allocator allocator)
        {
            return m_Provider.GetTextureDescriptors(
                default(XRTextureDescriptor),
                allocator);
        }

        /// <summary>
        /// Provides shader name used by <c>XRCameraSubsystem</c> to render texture.
        /// </summary>
        public string shaderName
        {
            get { return m_Provider.shaderName; }
        }

        /// <summary>
        /// Returns the camera intrinisics information.
        /// </summary>
        /// <param name="cameraIntrinsics">The camera intrinsics information returned from the method.</param>
        /// <returns>
        /// <c>true</c> if the method successfully gets the camera intrinsics information. Otherwise, <c>false</c>.
        /// </returns>
        public bool TryGetIntrinsics(out XRCameraIntrinsics cameraIntrinsics)
        {
            return m_Provider.TryGetIntrinsics(out cameraIntrinsics);
        }

        /// <summary>
        /// Method for the implementation to create the camera functionality provider.
        /// </summary>
        /// <returns>
        /// The camera functionality provider.
        /// </returns>
        protected abstract IProvider CreateProvider();

        /// <summary>
        /// Update the camera subsystem.
        /// </summary>
        public bool TryGetLatestFrame(
            XRCameraParams cameraParams,
            out XRCameraFrame frame)
        {
            if (m_Running && m_Provider.TryGetFrame(cameraParams, out frame))
            {
                return true;
            }

            frame = default(XRCameraFrame);
            return false;
        }

        /// <summary>
        /// Determines whether camera permission has been granted.
        /// </summary>
        /// <value><c>true</c> if permission has been granted;
        /// otherwise, <c>false</c>.</value>
        public bool permissionGranted
        {
            get { return m_Provider.permissionGranted; }
        }

        /// <summary>
        /// Registers a camera subsystem implementation based on the given subsystem parameters.
        /// </summary>
        /// <param name="cameraSubsystemParams">The parameters defining the camera subsystem functionality implemented
        /// by the subsystem provider.</param>
        /// <returns>
        /// <c>true</c> if the subsystem implementation is registered. Otherwise, <c>false</c>.
        /// </returns>
        /// <exception cref="System.ArgumentException">Thrown when the values specified in the
        /// <see cref="XRCameraSubsystemCinfo"/> parameter are invalid. Typically, this will occur
        /// <list type="bullet">
        /// <item>
        /// <description>if <see cref="XRCameraSubsystemCinfo.id"/> is <c>null</c> or empty</description>
        /// </item>
        /// <item>
        /// <description>if <see cref="XRCameraSubsystemCinfo.implementationType"/> is <c>null</c></description>
        /// </item>
        /// <item>
        /// <description>if <see cref="XRCameraSubsystemCinfo.implementationType"/> does not derive from the
        /// <see cref="XRCameraSubsystem"/> class
        /// </description>
        /// </item>
        /// </list>
        /// </exception>
        public static bool Register(XRCameraSubsystemCinfo cameraSubsystemParams)
        {
            XRCameraSubsystemDescriptor cameraSubsystemDescriptor = XRCameraSubsystemDescriptor.Create(cameraSubsystemParams);
            return SubsystemRegistration.CreateDescriptor(cameraSubsystemDescriptor);
        }
    }
}
