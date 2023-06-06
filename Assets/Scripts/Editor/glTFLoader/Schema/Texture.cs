//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace glTFLoader.Schema {
    using System.Linq;
    using System.Runtime.Serialization;
    
    
    public class Texture {
        
        /// <summary>
        /// Backing field for Sampler.
        /// </summary>
        private System.Nullable<int> m_sampler;
        
        /// <summary>
        /// Backing field for Source.
        /// </summary>
        private System.Nullable<int> m_source;
        
        /// <summary>
        /// Backing field for Name.
        /// </summary>
        private string m_name;
        
        /// <summary>
        /// Backing field for Extensions.
        /// </summary>
        private System.Collections.Generic.Dictionary<string, object> m_extensions;
        
        /// <summary>
        /// Backing field for Extras.
        /// </summary>
        private Extras m_extras;
        
        /// <summary>
        /// The index of the sampler used by this texture. When undefined, a sampler with repeat wrapping and auto filtering **SHOULD** be used.
        /// </summary>
        [Newtonsoft.Json.JsonPropertyAttribute("sampler")]
        public System.Nullable<int> Sampler {
            get {
                return this.m_sampler;
            }
            set {
                if ((value < 0)) {
                    throw new System.ArgumentOutOfRangeException("Sampler", value, "Expected value to be greater than or equal to 0");
                }
                this.m_sampler = value;
            }
        }
        
        /// <summary>
        /// The index of the image used by this texture. When undefined, an extension or other mechanism **SHOULD** supply an alternate texture source, otherwise behavior is undefined.
        /// </summary>
        [Newtonsoft.Json.JsonPropertyAttribute("source")]
        public System.Nullable<int> Source {
            get {
                return this.m_source;
            }
            set {
                if ((value < 0)) {
                    throw new System.ArgumentOutOfRangeException("Source", value, "Expected value to be greater than or equal to 0");
                }
                this.m_source = value;
            }
        }
        
        /// <summary>
        /// The user-defined name of this object.
        /// </summary>
        [Newtonsoft.Json.JsonPropertyAttribute("name")]
        public string Name {
            get {
                return this.m_name;
            }
            set {
                this.m_name = value;
            }
        }
        
        /// <summary>
        /// JSON object with extension-specific objects.
        /// </summary>
        [Newtonsoft.Json.JsonPropertyAttribute("extensions")]
        public System.Collections.Generic.Dictionary<string, object> Extensions {
            get {
                return this.m_extensions;
            }
            set {
                this.m_extensions = value;
            }
        }
        
        /// <summary>
        /// Application-specific data.
        /// </summary>
        [Newtonsoft.Json.JsonPropertyAttribute("extras")]
        public Extras Extras {
            get {
                return this.m_extras;
            }
            set {
                this.m_extras = value;
            }
        }
        
        public bool ShouldSerializeSampler() {
            return ((m_sampler == null) 
                        == false);
        }
        
        public bool ShouldSerializeSource() {
            return ((m_source == null) 
                        == false);
        }
        
        public bool ShouldSerializeName() {
            return ((m_name == null) 
                        == false);
        }
        
        public bool ShouldSerializeExtensions() {
            return ((m_extensions == null) 
                        == false);
        }
        
        public bool ShouldSerializeExtras() {
            return ((m_extras == null) 
                        == false);
        }
    }
}
