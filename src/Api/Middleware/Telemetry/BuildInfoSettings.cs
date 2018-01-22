using System;
using System.Collections.Generic;
using System.Linq;

namespace Api.Middleware.Telemetry
{
    public class BuildInfoSettings
    {
        public string AssemblyName { get; set; }

        public string Version { get; set; }

        public long BuildNumber
        {
            get
            {
                var parts = Version.Split('.');

                if (parts.Count() != 4)
                {
                    throw new ArgumentException($"Version number '{Version}' is invalid. It must have four parts, e.g. [int].[int].[int].[int]");
                }

                List<int> numberParts = null;

                try
                {
                    numberParts = parts.Select(x => Convert.ToInt32(x)).ToList();
                }
                catch (Exception ex)
                {
                    throw new ArgumentException($"Version number '{Version}' is invalid. Each part must be be made of integers.", ex);
                }

                if (numberParts.Any(x => x < 0))
                {
                    throw new ArgumentException($"Version number '{Version}' is invalid. Cannot contains negatives.");
                }

                // Assumes that we will never have more than 100 minor versions for any major.
                if (numberParts[1] > 99)
                {
                    throw new ArgumentException($"Version number '{Version}' is invalid. The minor version cannot be greater than 99.");
                }

                if (parts[2].Count() != 5)
                {
                    throw new ArgumentException($"Version number '{Version}' is invalid. Could not match third part to a date. Must be in format [yy][DayOfYear]. For example, 16002 represents 2nd January 2016.");
                }

                try
                {
                    var dayOfThisYear = new DateTime(Convert.ToInt32(parts[2].Substring(0, 2)), 1, 1).AddDays(Convert.ToInt32(parts[2].Substring(2)) - 1);
                }
                catch (Exception ex)
                {
                    throw new ArgumentException($"Version number '{Version}' is invalid. Could not match third part to a date. Must be in format [yy][DayOfYear]. For example, 16002 represents 2nd January 2016.", ex);
                }

                // Assumes that we will never have more than 100 revisions of the same major and minor on the same day.
                if (numberParts[3] > 99)
                {
                    throw new ArgumentException($"Version number '{Version}' is invalid. The revision number (the fourth part) cannot be greater than 99.");
                }

                return Convert.ToInt64($"{parts[0]}{numberParts[1].ToString("D2")}{parts[2]}{numberParts[3].ToString("D2")}");
            }
        }
    }
}