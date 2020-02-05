using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthBar : MonoBehaviour
{
    [SerializeField]private Vector2 Size = new Vector2(10,2);
    private SpriteRenderer Renderer;
    private MaterialPropertyBlock Block;
    void Start()
    {
        Renderer = GetComponent<SpriteRenderer>();
        Renderer.sprite = Sprite.Create(Texture2D.whiteTexture, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f));
        Block = new MaterialPropertyBlock();
        
        transform.localScale = Size;
        //IAttackable entity = GetComponentInParent<IAttackable>();
     //   entity.GetHealth.Bar = this;
       // entity.GetHealth.Entity = entity;
    }
    private float Health=1f, Hit=1f;
    public void UpdateHP(float hp)
    {
        Hit = Health;
        Health = hp;
    }
    void Update()
    {
        if(Hit>Health)
        {
            Hit -= 0.1f*Time.deltaTime;
        }
        Renderer.GetPropertyBlock(Block);
        Block.SetFloat("_Health", Health);
        Block.SetFloat("_Hit", Hit);
        Renderer.SetPropertyBlock(Block);
    }
}
