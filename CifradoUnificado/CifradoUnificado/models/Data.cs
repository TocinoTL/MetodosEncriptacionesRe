using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlgoritmoCifrado.Models
{
	public class Data
	{

		public string Filename { get; set; }
		public double Time { get; set; }
		public double TimePromedio { get; set; }
		public long memoriaUtilizada { get; set; }
		public float cpuUsage { get; set; }

		public Data(string filename, double time, double timePromedio, long memoriaUtilizada, float cpuUsage)
		{
			Filename = filename;
			Time = time;
			TimePromedio = timePromedio;
			this.memoriaUtilizada = memoriaUtilizada;
			this.cpuUsage = cpuUsage;
		}

		public Data()
		{
		}
	}
}
