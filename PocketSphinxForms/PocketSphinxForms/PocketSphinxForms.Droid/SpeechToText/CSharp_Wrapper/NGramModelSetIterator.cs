//------------------------------------------------------------------------------
// <auto-generated />
//
// This file was automatically generated by SWIG (http://www.swig.org).
// Version 3.0.12
//
// Do not make changes to this file unless you know what you are doing--modify
// the SWIG interface file instead.
//------------------------------------------------------------------------------

namespace SphinxBase {

public class NGramModelSetIterator : global::System.Collections.IEnumerator {
  private global::System.Runtime.InteropServices.HandleRef swigCPtr;
  protected bool swigCMemOwn;

  internal NGramModelSetIterator(global::System.IntPtr cPtr, bool cMemoryOwn) {
    swigCMemOwn = cMemoryOwn;
    swigCPtr = new global::System.Runtime.InteropServices.HandleRef(this, cPtr);
  }

  internal static global::System.Runtime.InteropServices.HandleRef getCPtr(NGramModelSetIterator obj) {
    return (obj == null) ? new global::System.Runtime.InteropServices.HandleRef(null, global::System.IntPtr.Zero) : obj.swigCPtr;
  }

  ~NGramModelSetIterator() {
    Dispose();
  }

  public virtual void Dispose() {
    lock(this) {
      if (swigCPtr.Handle != global::System.IntPtr.Zero) {
        if (swigCMemOwn) {
          swigCMemOwn = false;
          SphinxBasePINVOKE.delete_NGramModelSetIterator(swigCPtr);
        }
        swigCPtr = new global::System.Runtime.InteropServices.HandleRef(null, global::System.IntPtr.Zero);
      }
      global::System.GC.SuppressFinalize(this);
    }
  }

  public object Current 
  {
     get
     {
       return GetCurrent();
     }
  }

  public NGramModelSetIterator(SWIGTYPE_p_void ptr) : this(SphinxBasePINVOKE.new_NGramModelSetIterator(SWIGTYPE_p_void.getCPtr(ptr)), true) {
  }

  public bool MoveNext() {
    bool ret = SphinxBasePINVOKE.NGramModelSetIterator_MoveNext(swigCPtr);
    return ret;
  }

  public void Reset() {
    SphinxBasePINVOKE.NGramModelSetIterator_Reset(swigCPtr);
  }

  public NGramModel GetCurrent() {
    global::System.IntPtr cPtr = SphinxBasePINVOKE.NGramModelSetIterator_GetCurrent(swigCPtr);
    NGramModel ret = (cPtr == global::System.IntPtr.Zero) ? null : new NGramModel(cPtr, false);
    return ret;
  }

}

}
