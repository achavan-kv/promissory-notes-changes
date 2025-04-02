/* Version Number: 2.0
Date Changed: 12/10/2019 */

namespace Unicomer.Cosacs.Model
{
    public class JResponse
    {
        public string Result { get; set; }
        public dynamic Message { get; set; }
        public int StatusCode { get; set; }
        public bool Status { get; set; }
    }
}
