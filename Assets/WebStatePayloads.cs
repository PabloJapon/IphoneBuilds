using System.Collections.Generic;

[System.Serializable] public class WebDish { public string name; public string options; public string quantity; public string price; public int toggle; public string nota; public int orden; }
[System.Serializable] public class WebOrder { public int id; public string restaurant_id; public int mesa; public List<WebDish> dishes; }
[System.Serializable] public class WebOrdersResponse { public List<WebOrder> orders; }

[System.Serializable] public class WebDishState { public string name; public string options; public string quantity; public string price; }
[System.Serializable] public class WebDishStatus { public string name; public string options; public string quantity; public string price; public int state; }
[System.Serializable] public class MesaStatePayload { public string restaurant_id; public int mesa; public WebDishState[] previa; public WebDishStatus[] confirmed; public bool asistencia_active; public bool is_reset; }
[System.Serializable] public class MesaStateKey { public string restaurant_id; public int mesa; }
[System.Serializable] public class MesaStateKeysResponse { public List<MesaStateKey> keys; }