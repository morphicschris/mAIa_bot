using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordTest.StableDiffusion
{

    public class JsonSchema
    {
        public bool enable_hr { get; set; }
        public double denoising_strength { get; set; }
        //public int firstphase_width { get; set; }
        //public int firstphase_height { get; set; }
        public double hr_scale { get; set; }
        public string hr_upscaler { get; set; }
        //public int hr_second_pass_steps { get; set; }
        //public int hr_resize_x { get; set; }
        //public int hr_resize_y { get; set; }
        public string prompt { get; set; }
        //public List<string> styles { get; set; }
        public long seed { get; set; }
        //public int subseed { get; set; }

        //public double subseed_strength { get; set; }
        //public int seed_resize_from_h { get; set; }
        //public int seed_resize_from_w { get; set; }
        //public string sampler_name { get; set; }
        public int batch_size { get; set; }
        //public int n_iter { get; set; }
        public int steps { get; set; }
        public double cfg_scale { get; set; }
        public int width { get; set; }
        public int height { get; set; }
        public bool restore_faces { get; set; }
        //public bool tiling { get; set; }
        //public bool do_not_save_samples { get; set; }
        //public bool do_not_save_grid { get; set; }
        public string negative_prompt { get; set; }
        //public int eta { get; set; }
        //public int s_churn { get; set; }
        //public int s_tmax { get; set; }
        //public int s_tmin { get; set; }
        //public int s_noise { get; set; }
        //public Dictionary<string, object> override_settings { get; set; }
        //public bool override_settings_restore_afterwards { get; set; }
        //public List<string> script_args { get; set; }
        public string sampler_index { get; set; }
        //public string script_name { get; set; }
        //public bool send_images { get; set; }
        //public bool save_images { get; set; }
        //public Dictionary<string, object> alwayson_scripts { get; set; }
        //public List<ControlnetUnit> controlnet_units { get; set; }

        public string model { get; set; }
    }

    public class ControlnetUnit
    {
        public string input_image { get; set; }
        public string mask { get; set; }
        public string module { get; set; }
        public string model { get; set; }
        public int weight { get; set; }
        public string resize_mode { get; set; }
        public bool lowvram { get; set; }
        public int processor_res { get; set; }
        public int threshold_a { get; set; }
        public int threshold_b { get; set; }
        public int guidance { get; set; }
        public int guidance_start { get; set; }
        public int guidance_end { get; set; }
        public bool guessmode { get; set; }
    }

    public class Txt2ImgResponse
    {
        public List<string> images { get; set; }
        
        public Dictionary<string, object> parameters { get; set; }
        
        public string info { get; set; }
    }
}