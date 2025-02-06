using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebhookServer.Demo.Helpers
{
    internal static class Utilities
    {
        public static void ClearConsoleKeepLines(int linesToKeep)
        {
            // Lưu lại nội dung các dòng cần giữ
            string[] lines = new string[linesToKeep];
            for (int i = 0; i < linesToKeep; i++)
            {
                lines[i] = Console.ReadLine();
            }

            // Xoá toàn bộ màn hình console
            Console.Clear();

            // In lại các dòng đã lưu
            for (int i = 0; i < linesToKeep; i++)
            {
                Console.WriteLine(lines[i]);
            }
        }

    }
}
