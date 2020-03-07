using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PersonPanel : MonoBehaviour
{
    public ListFiller PersonFiller;
    public void Show(List<Person> persons)
    {
        PersonFiller.UpdateList(persons.ConvertAll(x => (object)x));
    }
}
