﻿using UnityEngine;
using System.Collections;

public class EnemyGeneral : CharacterGeneral
{
    public float f_Distance;
    public float f_Damage;
    public float delayTimer = 0f;
    public float shootDelayTime; //Enemy 총알 생성 속도

    public bool b_IsSearch = false;
    public bool b_IsPossibleLaserHit = true;

    

    public ParticleSystem ps;

    public GameObject Target;
    public GameObject Bullet;

    public GeneralInitialize.GunParameter EnemyWeapon;

    protected Vector3 v_TargetPosition;

    //Photon Value
    protected Vector3 v_NetworkTargetPos;

    protected virtual void Search(float dis)
    {

    }
    protected virtual void Trace()
    {
        
    }

    void Start()
    {
        ps = GetComponentInChildren<ParticleSystem>();
        ps.Stop();
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        var hit = col.gameObject;

        //총알과 충돌
        if (hit.layer == LayerMask.NameToLayer("Bullet"))
        {
            //데미지
            if (hit.GetComponent<BulletGeneral>().s_Victim == Util.S_ENEMY)
            {
                //유탄에 맞을 경우
                if (hit.tag == Util.S_GRENADE_NAME)
                {
                    bool IsMine = gameObject.GetComponentInParent<CharacterGeneral>().photonView.isMine;
                    if (IsMine)
                    {
                        EnemySound.instance.Play_Sound_Gun_Hit();
                        gameObject.GetComponent<PhotonView>().RPC("TakeDamageGrenade", PhotonTargets.All, col.gameObject.GetComponent<BulletGeneral>().bulletInfo.f_BulletDamage * f_Multiple);
                    }
                }

                //레이저에 맞을 경우
                else if (hit.tag == Util.S_LASER_NAME)
                {
                    bool IsMine = gameObject.GetComponentInParent<CharacterGeneral>().photonView.isMine;
                    if (IsMine && b_IsPossibleLaserHit == true)
                    {
                        EnemySound.instance.Play_Sound_Gun_Hit();
                        gameObject.GetComponent<PhotonView>().RPC("TakeDamageLaser", PhotonTargets.All, col.gameObject.GetComponentInChildren<BulletGeneral>().bulletInfo.f_BulletDamage * f_Multiple);
                    }
                }

                //그외 나머지(일반)
                else
                {
                    bool IsMine = gameObject.GetComponentInParent<CharacterGeneral>().photonView.isMine;
                    if (IsMine)
                    {
                        EnemySound.instance.Play_Sound_Gun_Hit();
                        gameObject.GetComponent<PhotonView>().RPC("TakeDamage", PhotonTargets.All, col.gameObject.GetComponent<BulletGeneral>().bulletInfo.f_BulletDamage * f_Multiple);
                    }
                }
            }
        }
    }

    //일반 공격
    [PunRPC]
    protected void TakeDamage(float _f_Damage)
    {
        if (this.n_hp > 0 && this.n_hp > _f_Damage)
        {
            this.n_hp -= _f_Damage;
            StartCoroutine("IsDamagedEnemy");
        }
        else
        {
            if (mySprite != null)
            {
                this.a_Animator.SetBool("Death", true);
                this.transform.Find("Sprite").GetComponent<SpriteRenderer>().enabled = false;
                this.transform.Find("Trigger").GetComponent<BoxCollider2D>().enabled = false;
                StartCoroutine(Death_Wait_Sec(0.5f));
            }
            else
            {
                //this.transform.Find("Sprite").GetComponent<MeshRenderer>().enabled = false;
                this.transform.Find("Trigger").GetComponent<CircleCollider2D>().enabled = false;
            }

            this.n_hp = 0;
            ps.Play();
            EnemySound.instance.Play_Sound_Explosion();  
        }
    }

    //특수 공격(레이저)
    [PunRPC]
    protected void TakeDamageLaser(float _f_Damage)
    {
        if (this.n_hp > 0 && this.n_hp > _f_Damage)
        {
            this.n_hp -= _f_Damage;
            StartCoroutine("Weaken");
            StartCoroutine("LaserHitCoolTime");
        }
        else
        {
            if (mySprite != null)
            {
                this.a_Animator.SetBool("Death", true);
                this.transform.Find("Sprite").GetComponent<SpriteRenderer>().enabled = false;
                this.transform.Find("Trigger").GetComponent<BoxCollider2D>().enabled = false;
                StartCoroutine(Death_Wait_Sec(0.5f));
            }
            else
            {
                //this.transform.Find("Sprite").GetComponent<MeshRenderer>().enabled = false;
                this.transform.Find("Trigger").GetComponent<CircleCollider2D>().enabled = false;
            }

            this.n_hp = 0;
            ps.Play();
            EnemySound.instance.Play_Sound_Explosion();
        }
    }

    //특수 공격(유탄)
    [PunRPC]
    protected void TakeDamageGrenade(float _f_Damage)
    {
        if (this.n_hp > 0 && this.n_hp > _f_Damage)
        {
            this.n_hp -= _f_Damage;
            StartCoroutine("IsDamagedEnemy");
            StartCoroutine("Burning");
        }
        else
        {
            if (mySprite != null)
            {
                this.a_Animator.SetBool("Death", true);
                this.transform.Find("Sprite").GetComponent<SpriteRenderer>().enabled = false;
                this.transform.Find("Trigger").GetComponent<BoxCollider2D>().enabled = false;
                StartCoroutine(Death_Wait_Sec(0.5f));
            }
            else
            {
                //this.transform.Find("Sprite").GetComponent<MeshRenderer>().enabled = false;
                this.transform.Find("Trigger").GetComponent<CircleCollider2D>().enabled = false;
            }

            this.n_hp = 0;
            ps.Play();
            EnemySound.instance.Play_Sound_Explosion();
        }
    }

    //타오름 효과
    protected IEnumerator Burning()
    {
        float Timer = 0;
        yield return new WaitForSeconds(1.0f);
        if (this.n_hp > 0 && this.n_hp > Util.F_BURNING)
        {
            while (true)
            {
                Timer += 1.0f;
                this.n_hp -= Util.F_BURNING;
                StartCoroutine("IsDamagedEnemy");

                if (this.n_hp - Util.F_BURNING <= 0)
                {
                    //불탐 효과로는 적을 죽일수 없음
                    this.n_hp = 1;
                }
                else
                {
                    yield return new WaitForSeconds(1.0f);
                }

                if (Timer == 5.0f)
                {
                    break;
                }
            }
        }

        yield return null;
    }
    

    protected IEnumerator LaserHitCoolTime()
    {
        b_IsPossibleLaserHit = false;

        yield return new WaitForSeconds(Util.F_LASER_HIT_COOLTIME);

        b_IsPossibleLaserHit = true;
    }
}
