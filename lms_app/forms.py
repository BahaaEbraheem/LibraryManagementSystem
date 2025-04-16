from django import forms
from .models import Book


class bookForm(forms.ModelForm):
    class Meta:
        model = Book
        fields = '__all__'
        # wedgets={
        #     'title':forms.TextInput(attrs={'class':'form-control'}),
        #     'author':forms.TextInput(attrs={'class':'form-control'}),
        #     'photo_book':forms.FileField(attrs={'class':'form-control'}),
        #     'category':forms.Select(attrs={'class':'form-control'}),
        # }
        