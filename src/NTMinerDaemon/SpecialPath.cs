﻿using System.IO;

namespace NTMiner {
    public static class SpecialPath {
        public static readonly string NTMinerLocalJsonFileFullName = Path.Combine(VirtualRoot.HomeDirFullName, "local.json");
        public static readonly string NTMinerServerJsonFileFullName = Path.Combine(VirtualRoot.HomeDirFullName, "server.json");
        public static string GpuProfilesJsonFileFullName = Path.Combine(VirtualRoot.HomeDirFullName, "gpuProfiles.json");
        public static string ReadGpuProfilesJsonFile() {
            if (File.Exists(GpuProfilesJsonFileFullName)) {
                return File.ReadAllText(GpuProfilesJsonFileFullName);
            }

            return string.Empty;
        }

        public static void SaveGpuProfilesJsonFile(string json) {
            File.WriteAllText(GpuProfilesJsonFileFullName, json);
        }
    }
}
