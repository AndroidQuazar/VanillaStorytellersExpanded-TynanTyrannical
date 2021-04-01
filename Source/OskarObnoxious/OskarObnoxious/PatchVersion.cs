using System;
using System.Globalization;
using Verse;

namespace OskarObnoxious
{
    public struct PatchVersion
    {
        private const int MaxPatchRevision = 1500;
        private const int NextMinorVersion = 9999;
        private const int NextMajorVersion = 9;

        private int major;
        private int minor;
        private int patch;

        public PatchVersion(int major, int minor, int patch)
        {
            this.major = major;
            this.minor = minor;
            this.patch = patch;
        }

        private static int NewPatchVersion => Rand.RangeInclusive(GameComponent_PatchNotes.Instance.latestVersion.patch, GameComponent_PatchNotes.Instance.latestVersion.patch + MaxPatchRevision);

        public bool IsValid => major > 0 && major <= NextMajorVersion && minor >= 0 && minor <= NextMajorVersion && patch >= 0 && patch <= NextMinorVersion;

        public static PatchVersion Default => new PatchVersion(1, 0, 0);

        public static PatchVersion NextVersion
        {
            get
            {
                int major = 1;
                int minor = 0;
                int patch = NewPatchVersion;
                if (GameComponent_PatchNotes.Instance.latestVersion.IsValid)
                {
                    major = GameComponent_PatchNotes.Instance.latestVersion.major;
                    minor = GameComponent_PatchNotes.Instance.latestVersion.minor;
                    if (patch > NextMinorVersion)
                    {
                        minor += 1;
                        patch -= NextMinorVersion;
                        if (minor > NextMajorVersion)
                        {
                            major += 1;
                            if (major > NextMajorVersion)
                            {
                                minor = 0;
                                major = 0;
                                patch = 0;
                            }
                        }
                    }
                }
                    
                PatchVersion nextVersion = new PatchVersion(major, minor, patch);
                GameComponent_PatchNotes.Instance.latestVersion = nextVersion;
                return nextVersion;
            }
        }

        public static PatchVersion FromString(string version)
        {
            string[] data = version.Split('.');

            try
            {
                CultureInfo invariantCulture = CultureInfo.InvariantCulture;
	            int major = Convert.ToInt32(data[0], invariantCulture);
	            int minor = Convert.ToInt32(data[1], invariantCulture);
                int patch = Convert.ToInt32(data[2], invariantCulture);
                return new PatchVersion(major, minor, patch);
            }
            catch (Exception)
            {
                Log.Warning($"{version} is not a valid version for PatchNotes. Resetting to 1.0.0");
                return Default;
            }
        }

        public override string ToString()
        {
            return string.Format("{0}.{1}.{2}", major, minor, patch.ToString("D4"));
        }
    }
}
